/****************************************************************************

Copyright 2016 liangxiegame@163.com
Copyright 2016 sophieml1989@gmail.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

****************************************************************************/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    public class Workspace
    {
        /// <summary>
        /// The workspace's id
        /// </summary>
        public string Id;
        
        public class WorkspaceOptions
        {
            public bool RTL = false;
            public int MaxBlocks = -1;
            public bool ReadOnly = false;
        }
        
        public WorkspaceOptions Options { get; private set; }

        public bool RTL
        {
            get { return Options.RTL; }
        }
        
        public List<Block> TopBlocks { get; private set; }
        public Dictionary<string,Block> BlockDB { get; private set; }
        public VariableMap VariableMap { get; private set; }
        public Dictionary<Define.EConnection, ConnectionDB> ConnectionDBList { get; private set; }
        public ProcedureDB ProcedureDB { get; private set; }
                
        /// <summary>
        /// Maximum number of undo events in stack. '0' turnes off udo,"Infinity" sets  it to unlimited.
        /// </summary>
        private const int MAX_UNDO = 1024;
        
        /// <summary>
        /// Angle away from the horizontal to sweep for blocks.  Order of execution is
        /// generally top to bottom, but a small angle changes the scan to give a bit of
        /// a left to right bias (reversed in RTL).  Units are in degrees.
        /// </summary>
        private const int SCAN_ANGLE = 3;
        
        /// <summary>
        /// Class for a workspace. This is a data structure tha contains blocks.
        /// There is no UI,and can be created headlessly.
        /// </summary>
        public Workspace(WorkspaceOptions options = null, string optId = null)
        {
            if (string.IsNullOrEmpty(optId))
            {
                Id = Utils.GenUid();
            }
            else
            {
                Id = optId;
            }

            if (mWorkspaceDB.ContainsKey(Id))
            {
                mWorkspaceDB[Id] = this;
                Debug.LogWarning("Already contains workspace id:" + Id);
            }
            else
            {
                mWorkspaceDB.Add(Id, this);
            }

            Options = options ?? new WorkspaceOptions();

            TopBlocks = new List<Block>();
            BlockDB = new Dictionary<string, Block>();
            VariableMap = new VariableMap(this);
            ConnectionDBList = ConnectionDB.Build();
            ProcedureDB = new ProcedureDB(this);
        }
        
        /// <summary>
        /// Dispose of this workspace.
        /// Unlink from all DOM elements to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            //this.mListeners.Clear();
            this.Clear();
            // Remove from workspace database.
            mWorkspaceDB.Remove(this.Id);
        }
        
        public void Clear()
        {
            while (TopBlocks.Count > 0)
            {
                TopBlocks[TopBlocks.Count - 1].Dispose();
            }
            
            VariableMap.Clear();
            ConnectionDBList.Clear();
            ProcedureDB.Clear();
        }
        
        #region Blocks

        /// <summary>
        /// Obtain a newly created block.
        /// </summary>
        /// <param name="prototypeName"> Name of the language object containing
        /// type-specific functions for this block.</param>
        /// <param name="opt_id"> Optional ID. Use this ID if provided,otherwise
        /// create a new id.</param>
        /// <returns>The created block.</returns>
        public Block NewBlock(string prototypeName, string opt_id = null)
        {
            return BlockFactory.Instance.CreateBlock(this, prototypeName, opt_id);
        }

        /// <summary>
        /// Find the block on this workspace with the specified ID.
        /// </summary>
        public Block GetBlockById(string id)
        {
            Block block = null;
            BlockDB.TryGetValue(id, out block);
            return block;
        }

        /// <summary>
        /// Add a block to the list of top blocks.
        /// </summary>
        /// <param name="block">Block to add.</param>
        public void AddTopBlock(Block block)
        {
            if (!TopBlocks.Contains(block))
                TopBlocks.Add(block);
            
            // deal with procedure blocks
            if (ProcedureDB.IsDefinition(block)) ProcedureDB.AddDefinition(block);
            else if (ProcedureDB.IsCaller(block)) ProcedureDB.AddCaller(block);
        }

        /// <summary>
        /// Remove a block from the list of top blocks.
        /// </summary>
        /// <param name="block">Block to remove</param>
        public void RemoveTopBlock(Block block)
        {
            TopBlocks.Remove(block);
            
            // deal with procedure blocks
            if (ProcedureDB.IsDefinition(block)) ProcedureDB.RemoveDefinition(block);
            else if (ProcedureDB.IsCaller(block)) ProcedureDB.RemoveCaller(block);
        }

        /// <summary>
        /// Finds the top-level blocks and returns them. Blocks are optionally sorted
        /// by position; top to bottom (with slight LTR or RTL bias).
        /// @param {boolean} ordered Sort the list if true.
        /// @return {!Array.<!Blockly.Block>} The top-level block objects.
        /// </summary>
        /// <param name="ordered"></param>
        public List<Block> GetTopBlocks(bool ordered)
        {
            // Copy the topBlocks_list.
            var blocks = new List<Block>();
            blocks.AddRange(TopBlocks);
            if (ordered && blocks.Count > 1)
            {
                var offset = Math.Sin(Workspace.SCAN_ANGLE * Mathf.Deg2Rad);
                if (this.RTL)
                {
                    offset *= -1;
                }
                blocks.Sort(delegate(Block a, Block b)
                {
                    var aXY = a.XY;
                    var bXY = b.XY;
                    return (int)((aXY.y + offset * aXY.x) - (bXY.y + offset * bXY.x));
                });
            }
            return blocks;
        }

        /// <summary>
        /// Find all blocks in workspace. No particular order.
        /// @return {!Array.<!Blockly.Block>} Array of blocks.
        /// </summary>
        /// <returns></returns>
        public List<Block> GetAllBlocks()
        {
            var blocks = this.GetTopBlocks(false);
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks.AddRange(blocks[i].GetChildren());
            }
            return blocks;
        }
        #endregion
        
        
        #region Variables
        
        /// <summary>
        /// Create a variable with a given name,optional type, and optional id.
        /// </summary>
        /// <param name="name">The name of the variable.This must be unique across
        /// variables and procedures.</param>
        /// <param name="optType">The type of the variable like 'int' or 'string'.
        /// Does not need to be unique. Field_variable can filter variables based on 
        /// theri type. This will default to '' which is a specific type.</param>
        /// <param name="optId">The unique id of the variable. This will default to a UUID.</param>
        /// <returns>VariableModel ,The newly created variable.</returns>
        public VariableModel CreateVariable(string name, string optType = null, string optId = null)
        {
            return this.VariableMap.CreateVariable(name, optType, optId);
        }

        /// <summary>
        /// Check if exist the variable by the given name.
        /// </summary>
        /// <param name="name"> The name to check for.</param>
        public bool HasVariable(string name)
        {
            return GetVariable(name) != null;
        }
        
        /// <summary>
        /// Find the variable by the given name and return it. Return null if it is not found.
        /// </summary>
        /// <param name="name"> The name to check for.</param>
        public VariableModel GetVariable(string name)
        {
            return VariableMap.GetVariable(name);
        }
        
        /// <summary>
        /// Find the variable by the given id and return it. Return null if it is not found.
        /// </summary>
        /// <param name="id"> id The id to check for.</param>
        public VariableModel GetVariableById(string id)
        {
            return VariableMap.GetVariableById(id);
        }

        /// <summary>
        /// Find the variable with the specified type.If type is null,return listof
        /// variables with empty string type.
        /// </summary>
        /// <param name="type"> Type of the variables to find.</param>
        /// <returns>The sought after variables of the passed in type.An empty array if none are found</returns>
        public List<VariableModel> GetVariablesOfType(string type)
        {
            return this.VariableMap.GetVariablesOfType(type);
        }

        public List<string> GetVariableTypes ()
        {
            return VariableMap.GetVariableTypes();
        }
        
        public List<VariableModel> GetAllVariables ()
        {
            return VariableMap.GetAllVariables();
        }
        
        /// <summary>
        /// Find all the uses of a named variable.
        /// </summary>
        /// <param name="name"> Name of variable.</param>
        /// <returns> Array of block usages.</returns>
        public List<Block> GetVariableUses(string name)
        {
            var uses = new List<Block>();
            var blocks = this.GetAllBlocks();
            // Iterate through every block and check the name
            foreach (var block in blocks)
            {
                var blockVariables = block.GetVars();
                if (null != blockVariables && blockVariables.Count != 0)
                {
                    foreach (var varName in blockVariables)
                    {    
                        // Variable name may be null if the block is only half-built.
                        if (null != varName && null != name && Names.Equals(varName, name))
                        {
                            uses.Add(block);
                        }   
                    }
                }
            }
            return uses;
        }

        /// <summary>
        /// Delete a variable by the passed in name and all of its uses from this
        /// workspace.May prompt the user for confirmation.
        /// </summary>
        /// <param name="name"> Name of variable to delete.</param>
        public void DeleteVariable(string name)
        {
            // Check wether this variable is a function parameter before deleting.
            var uses = this.GetVariableUses(name);
            foreach (var block in uses)
            {
                if (string.Equals(block.Type, Define.DEFINE_NO_RETURN_BLOCK_TYPE) ||
                    string.Equals(block.Type, Define.DEFINE_WITH_RETURN_BLOCK_TYPE))
                {
                    var procedureName = block.GetFieldValue("NAME");
                    Debug.LogError("Alert:" + I18n.Msg[MsgDefine.CANNOT_DELETE_VARIABLE_PROCEDURE].
                                       Replace("%1",name).
                                       Replace("%2",procedureName));
                    return;
                }
            }

            var workspace = this;
            var variable = workspace.GetVariable(name);
            if (uses.Count > 1)
            {
                // Confirm before deleting multiple blocks.
                Debug.Log("confirm:" + I18n.Msg[MsgDefine.DELETE_VARIABLE_CONFIRMATION]
                              .Replace("%1", uses.Count.ToString()).Replace("%2", name));
                workspace.DeleteVariableInternal(variable);
            }
            else
            {
                // No confirmation necessary for a single block.
                this.DeleteVariableInternal(variable);
            }
        }

        /// <summary>
        /// Delete a variables by the passed in id and all of its uses from this
        /// workspace.May prompt the user for confirmation.
        /// </summary>
        /// <param name="id"> Id of variable to delete.</param>
        public void DeleteVariableById(string id)
        {
            var variable = this.GetVariableById(id);
            if (null != variable)
            {
                this.DeleteVariableInternal(variable);
            }
            else
            {
                Debug.LogError("Can't delete non-existant variable: " + id);
            }
        }
 
        /// <summary>
        /// Deletes a variable and all of its uses from this workspace without asking the
        /// user for confirmation.
        /// </summary>
        /// <param name="variable"> Variable to delete</param>
        public void DeleteVariableInternal(VariableModel variable)
        {
            var uses = GetVariableUses(variable.Name);
            foreach (var block in uses)
            {
                block.Dispose(true);
            }
            VariableMap.DeleteVariable(variable);
        }

        /// <summary>
        /// Rename a variable by udpate its name in the variable map.Identify the 
        /// variable to rename with the given variable.
        /// </summary>
        /// <param name="variable">Variable to rename</param>
        /// <param name="newName">New variable name</param>
        public void RenameVariableInternal(VariableModel variable, string newName)
        {
            var newVariable = this.GetVariable(newName);
            
            // If they are different types, throw an error.
            if (null != variable && null != newVariable && !string.Equals(variable.Type, newVariable.Type))
            {
                Debug.LogError("Variable " + variable.Name + " is type " + variable.Type +
                               " and variable " + newName + " is type " + newVariable.Type +
                               ".Both must be the same type.");
                return;
            }

            string oldName = variable != null ? variable.Name : null;
            string oldCase = newVariable != null ? newVariable.Name : null;
            
            this.VariableMap.RenameVariable(variable, newName);
            
            // Iterate through every block and update name.
            var blocks = this.GetAllBlocks();
            foreach (var block in blocks)
            {
                block.RenameVar(oldName, newName);
                // newVariable's name maybe changed after renaming, because of the case insensative
                if (!string.IsNullOrEmpty(oldCase) && !oldCase.Equals(newName))
                {
                    block.RenameVar(oldCase, newName);
                }
            }
        }

        /// <summary>
        /// Rename a variable by updating its name in the variable map.Identify the
        /// variable to rename with the given name.
        /// </summary>
        /// <param name="oldName"> Variable to rename</param>
        /// <param name="newName"> New variable name.</param>
        public void RenameVariable(string oldName, string newName)
        {
            // Warning: Prefer to use renameVariableById.
            var variable = this.GetVariable(oldName);
            this.RenameVariableInternal(variable, newName);
        }

        /// <summary>
        /// Rename a variable by updating its name in the variable map.Identify the
        /// variable to rename with the given id
        /// </summary>
        /// <param name="id"> Id of the variable to rename</param>
        /// <param name="newName"> New variable name.</param>
        public void RenameVariableById(string id, string newName)
        {
            var variable = this.GetVariableById(id);
            this.RenameVariableInternal(variable,newName);
        }
        
        /// <summary>
        /// Walk the workspace and update the map of variables to only contain ones in
        /// use on the workspace. Use when loading new workspaces from disk.
        /// </summary>
        /// <param name="clear"> True if the old variable map should be cleared.</param>
        public void UpdateVariableStore(bool clear = false,List<string> unitTestAllUsedVariable = null)
        {
            var variableNames =  unitTestAllUsedVariable == null ? Variables.AllUsedVariables(this) : unitTestAllUsedVariable;
            var varList = new List<JObject>();
            foreach (var name in variableNames)
            {
                // Get variable model with the used variable name.
                var tempVar = GetVariable(name);
                if (null != tempVar)
                {
                    JObject jsonData = new JObject();
                    jsonData["name"] = tempVar.Name;
                    jsonData["type"] = tempVar.Type;
                    jsonData["id"] = tempVar.ID;
                    varList.Add(jsonData);
                }
                else
                {
                    JObject jsonData = new JObject();
                    jsonData["name"] = name;
                    jsonData["type"] = string.Empty;
                    jsonData["id"] = string.Empty;
                    varList.Add(jsonData);
                    // instances are storing more than just name.
                }
            }
            
            if (clear) VariableMap.Clear();
            
            // Update the list in place so that the flyout's references stay correct.
            foreach (var varDict in varList)
            {
                if (null == this.GetVariable(varDict["name"].ToString()))
                {
                    this.CreateVariable(varDict["name"].ToString(),varDict["type"].ToString(),varDict["id"].ToString());
                }
            }
        }

        #endregion 
        
        static Dictionary<string,Workspace> mWorkspaceDB = new Dictionary<string, Workspace>();

        /// <summary>
        /// Find the workspace with the specified ID.
        /// </summary>
        /// <param name="id"> ID of workspace to find.</param>
        /// <returns></returns>
        public static Workspace GetByID(string id)
        {
            Workspace workspace = null;
            mWorkspaceDB.TryGetValue(id, out workspace);
            return workspace;
        }
    }
}
