/****************************************************************************

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
using System.Linq;

namespace UBlockly
{
    public class ProcedureUpdateData
    {
        public const int Add = 1;
        public const int Remove = 2;
        public const int Mutate = 3;

        public readonly int Type;
        
        /// <summary>
        /// the procedure info which the update is concerned
        /// </summary>
        public readonly Procedure ProcedureInfo;
        
        /// <summary>
        /// set when adding a definition block 
        /// </summary>
        public readonly Block ProcedureDefinitionBlock;
        
        /// <summary>
        /// set when removing a definition block
        /// </summary>
        public readonly List<Block> ProcedureCallerBlocks;

        /// <summary>
        /// new procedure info for mutating procedures
        /// </summary>
        public readonly Procedure NewProcedureInfo;

        private ProcedureUpdateData(int type, Procedure procedure)
        {
            Type = type;
            ProcedureInfo = procedure;
        }

        public ProcedureUpdateData(Procedure procedure, Block defBlock) : this(Add, procedure)
        {
            ProcedureDefinitionBlock = defBlock;
        }

        public ProcedureUpdateData(Procedure procedure, List<Block> callerBlocks) : this(Remove, procedure)
        {
            ProcedureCallerBlocks = callerBlocks;
        }

        public ProcedureUpdateData(Procedure oldProcedure, Procedure newProcedure) : this(Mutate, oldProcedure)
        {
            NewProcedureInfo = newProcedure;
        }
    }
    
    /// <summary>
    /// Procedure manager for a workspace
    /// </summary>
    public class ProcedureDB : Observable<ProcedureUpdateData>
    {
        public const string PROCEDURE_NAME_FIELD = "NAME";
        public const string STATEMENT_INPUT_NAME = "STACK";
        public const string RETURN_INPUT_NAME = "RETURN";
        public const bool HAS_STATEMENTS_DEFAULT = true;

        public const string DEFINE_NO_RETURN_BLOCK_TYPE = Define.DEFINE_NO_RETURN_BLOCK_TYPE;
        public const string DEFINE_WITH_RETURN_BLOCK_TYPE = Define.DEFINE_WITH_RETURN_BLOCK_TYPE;
        public const string CALL_NO_RETURN_BLOCK_TYPE = Define.CALL_NO_RETURN_BLOCK_TYPE;
        public const string CALL_WITH_RETURN_BLOCK_TYPE = Define.CALL_WITH_RETURN_BLOCK_TYPE;
        public const string IF_RETURN_BLOCK_TYPE = "procedures_ifreturn";

        private Workspace mWorkspace;
        private Names mNameMgr;
        private Dictionary<string, Block> mProcedureDefinitions;
        private Dictionary<string, List<Block>> mProcedureCallers;

        public ProcedureDB(Workspace workspace)
        {
            mWorkspace = workspace;
            mNameMgr = new Names(null);
            mProcedureDefinitions = new Dictionary<string, Block>();
            mProcedureCallers = new Dictionary<string, List<Block>>();
        }
        
        public void Clear()
        {
            mWorkspace = null;
            mNameMgr.Reset();
            mProcedureDefinitions.Clear();
            mProcedureCallers.Clear();
        }
        
        /// <summary>
        /// Determines if a block is procedure call.
        /// </summary>
        public static bool IsCaller(Block block)
        {
            return block.Mutator is ProcedureCallMutator;
        }

        /// <summary>
        /// Determines if a block is procedure definition.
        /// </summary>
        public static bool IsDefinition(Block block)
        {
            return block.Mutator is ProcedureDefinitionMutator;
        }

        /// <summary>
        /// Determines if a procedure definition block has return
        /// </summary>
        public static bool HasReturn(Block block)
        {
            return IsDefinition(block) && block.Type.Equals(DEFINE_WITH_RETURN_BLOCK_TYPE);
        }

        /// <summary>
        /// If the block is a procedure definition or procedure call/reference, it returns the name of the procedure.
        /// </summary>
        public static string GetProcedureName(Block block)
        {
            ProcedureMutator procedure = block.Mutator as ProcedureMutator;
            return procedure == null ? null : procedure.GetProcedureName();
        }

        /// <summary>
        /// name or rename a procedure for a single block.
        /// </summary>
        public static void SetProcedureName(Block block, string newName)
        {
            ProcedureMutator procedure = block.Mutator as ProcedureMutator;
            if (procedure != null)
                procedure.SetProcedureName(newName);
        }
        
        /// <summary>
        /// If the block is a procedure block, returns the argument list. Otherwise, it returns null.
        /// </summary>
        public static List<string> GetProcedureArguments(Block block)
        {
            ProcedureMutator procedure = block.Mutator as ProcedureMutator;
            return procedure == null ? null : procedure.GetArgumentNameList();
        }

        /// <summary>
        /// All procedure definition blocks
        /// </summary>
        public List<Block> GetDefinitionBlocks()
        {
            return mProcedureDefinitions.Values.ToList();
        }

        /// <summary>
        /// Check if exist a procedure definition block named procedureName
        /// </summary>
        public bool ContainsDefinition(string procedureName)
        {
            return mProcedureDefinitions.ContainsKey(procedureName);
        }

        /// <summary>
        /// Find the definition block for the named procedure.
        /// </summary>
        public Block GetDefinitionBlock(string procedureName)
        {
            Block block;
            mProcedureDefinitions.TryGetValue(procedureName, out block);
            return block;
        }
        
        /// <summary>
        /// Adds a block containing a procedure definition to the managed list.  If a procedure
        /// by that name is already defined, creates a new unique name for the procedure and renames the block.
        /// </summary>
        public void AddDefinition(Block block)
        {
            string procedureName = GetProcedureName(block);
            if (string.IsNullOrEmpty(procedureName) || mProcedureDefinitions.ContainsKey(procedureName))
                return;
            
            procedureName = mNameMgr.GetDistinctName(procedureName);
            SetProcedureName(block, procedureName);
            mProcedureDefinitions.Add(procedureName, block);
            mProcedureCallers.Add(procedureName, new List<Block>());

            FireUpdate(new ProcedureUpdateData(((ProcedureMutator) block.Mutator).ProcedureInfo, block));
        }

        /// <summary>
        /// Removes the block containing the procedure definition, and removes all callers as well
        /// </summary>
        /// <returns>A list of Blocks that call the procedure defined by block.</returns>
        public List<Block> RemoveDefinition(Block block)
        {
            string procedureName = GetProcedureName(block);
            if (string.IsNullOrEmpty(procedureName) || !mProcedureDefinitions.ContainsKey(procedureName))
                return null;

            List<Block> callers = mProcedureCallers[procedureName];
            mProcedureDefinitions.Remove(procedureName);
            mProcedureCallers.Remove(procedureName);
            mNameMgr.RemoveDistinctName(procedureName);

            FireUpdate(new ProcedureUpdateData(((ProcedureMutator) block.Mutator).ProcedureInfo, callers));
            return callers;
        }
        
        /// <summary>
        /// Find all the callers of a named procedure.
        /// </summary>
        public List<Block> GetCallers(string procedureName)
        {
            List<Block> callers;
            mProcedureCallers.TryGetValue(procedureName, out callers);
            return callers;
        }

        /// <summary>
        /// Check if the procedure block is called one or more times.
        /// </summary>
        public bool HasCallers(Block block)
        {
            string procedureName = GetProcedureName(block);
            if (string.IsNullOrEmpty(procedureName))
                return false;
            List<Block> callers = GetCallers(procedureName);
            return callers != null && callers.Count > 0;
        }

        /// <summary>
        /// Add a caller to a procedure
        /// </summary>
        public void AddCaller(Block block)
        {
            string procedureName = GetProcedureName(block);
            if (string.IsNullOrEmpty(procedureName))
                return;
            
            List<Block> callers;
            mProcedureCallers.TryGetValue(procedureName, out callers);
            if (callers != null)
            {
                callers.Add(block);
            }
        }

        /// <summary>
        /// Remove a caller to a procedure
        /// </summary>
        public void RemoveCaller(Block block)
        {
            string procedureName = GetProcedureName(block);
            if (string.IsNullOrEmpty(procedureName))
                return;
            
            List<Block> callers;
            mProcedureCallers.TryGetValue(procedureName, out callers);
            if (callers != null)
            {
                callers.Remove(block);
            }
        }

        /// <summary>
        /// Updates all blocks related to a specific procedure with respect to name, arguments, and
        /// whether the definition can contain a statement sequence. If any of the optional arguments are
        /// null, the existing values from the blocks are used.
        /// </summary>
        /// <param name="originalProcedureName">The name of the procedure, before this method.</param>
        /// <param name="updatedProcedure">The info with which to update procedure mutators.</param>
        /// <param name="argIndexUpdates">A list of mappings from original argument index to new index.</param>
        public void MutateProcedure(string originalProcedureName, Procedure updatedProcedure,
                                    List<ProcedureArgumentIndexUpdate> argIndexUpdates = null)
        {
            Block definition = GetDefinitionBlock(originalProcedureName);
            if (definition == null)
                throw new Exception("Unknown procedure \"" + originalProcedureName + "\"");
            
            List<Block> procedureCalls = GetCallers(originalProcedureName);
            ProcedureDefinitionMutator definitionMutator = definition.Mutator as ProcedureDefinitionMutator;
            Procedure oldInfo = definitionMutator.ProcedureInfo;

            mNameMgr.RemoveDistinctName(originalProcedureName);
            string newProcedureName = mNameMgr.GetDistinctName(updatedProcedure.Name);
            bool isFuncRename = !newProcedureName.Equals(originalProcedureName);
            if (isFuncRename)
            {
                mProcedureDefinitions.Remove(originalProcedureName);
                mProcedureDefinitions[newProcedureName] = definition;
                mProcedureCallers.Remove(originalProcedureName);
                mProcedureCallers[newProcedureName] = procedureCalls;
            }

            List<string> newArgs = updatedProcedure.Arguments;
            for (int i = 0; i < newArgs.Count; i++)
            {
                string argName = newArgs[i];
                if (!Names.IsSafe(argName))
                    throw new Exception("Invalid variable name \"" + argName + "\" " + "(argument #" + i + " )");
                
                // create new variable
                if (!mWorkspace.HasVariable(argName))
                    mWorkspace.CreateVariable(argName);
            }
            
            definitionMutator.Mutate(updatedProcedure);
            
            //mutate each procedure call
            foreach (Block procRef in procedureCalls)
            {
                ProcedureCallMutator callMutator = procRef.Mutator as ProcedureCallMutator;
                int oldArgCount = callMutator.GetArgumentNameList().Count;
                Block[] oldValues = new Block[oldArgCount];
                
                //disconnect prior value blocks
                for (int i = 0; i < oldArgCount; i++)
                {
                    Input argInput = callMutator.GetArgumenInput(i);
                    Block valueBlock = argInput.ConnectedBlock;
                    if (valueBlock != null)
                    {
                        oldValues[i] = valueBlock;
                        argInput.Connection.Disconnect();
                    }
                }
                
                callMutator.Mutate(updatedProcedure);
                
                //reconnect any blocks to orginal inputs
                if (argIndexUpdates != null)
                {
                    foreach (ProcedureArgumentIndexUpdate indexUpdate in argIndexUpdates)
                    {
                        Block originalValue = oldValues[indexUpdate.Before];
                        if (originalValue != null)
                        {
                            Input argInput = callMutator.GetArgumenInput(indexUpdate.After);
                            argInput.Connection.Connect(originalValue.OutputConnection);
                        }
                    }
                }
            }
            
            FireUpdate(new ProcedureUpdateData(oldInfo, updatedProcedure));
        }
    }
}
