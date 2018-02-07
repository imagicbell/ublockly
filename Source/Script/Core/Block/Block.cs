/****************************************************************************

Copyright 2016 sophieml1989@gmail.com
Copyright 2016 liangxiegame@163.com

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using UBlockly;
using UnityEngine;
using UnityEngine.Assertions;

namespace UBlockly
{
    /***************************
     hierarchy of block:
     - Block(Topmost in workspace)
       - ConnectionOutput
       - ConnectionPrev  
       - ConnectionNext
         - Block(Next)
         
       - Input
         - Field 
         - Field 
         ...
         - ConnectionInput
           - Block(Input)
       - Input
         ...
       
     - Block
       ...             
    ***************************/
    
    /// <summary>
    /// Block core model class
    /// inherit from Observable, where int is the UpdateState mask
    /// </summary>
    public class Block : Observable<int>
    {
        public enum UpdateState
        {
            Inputs = 0,
            //Fields = 1,
            Connections = 2,
            IsDisabled = 3,
            IsCollapsed = 4,
            IsEditable = 5,
            IsDeletable = 6,
            IsMovable = 7,
            IsInputInline = 8,
            IsShadow = 9,
            
            //---- max 31 (mask int) ------
        }
        
        public string Type { get; protected set; }
        public string ID { get; protected set; }
        public Workspace Workspace { get; set; }

        public Connection OutputConnection { get; set; }
        public Connection NextConnection { get; set; }
        public Connection PreviousConnection { get; set; }
        public List<Input> InputList { get; protected set; }
        public Mutator Mutator { get; protected set; }

        public Block ParentBlock { get; protected set; }
        public List<Block> ChildBlocks = new List<Block>();
        
        /// <summary>
        /// The block's position in workspace units.  (0, 0) is at the workspace's origin; scale does not change this value.
        /// </summary>
        public Vector2 XY { get; set; }
        
        /// <summary>
        /// Check if the block in Right to Left direction
        /// </summary>
        public bool RTL
        {
            get { return Workspace != null && Workspace.RTL; }
        }

        public string Data = null;    
        
        public Block() {}

        /// <summary>
        /// Class for one block.
        /// Not normally called directly,workspace.newBlock() is preferred.
        /// </summary>
        /// <param name="workspace"> The Block's workspace</param>
        /// <param name="prototypeName"> Name of the language object containing
        /// type-specific functions for this block. </param>
        /// <param name="opt_id">Use this ID if provided,otherwise create a new id</param>
        public Block(Workspace workspace, string prototypeName = null, string opt_id = null)
        {
            Type = prototypeName;
            ID = !string.IsNullOrEmpty(opt_id) && workspace.GetBlockById(opt_id) == null
                  ? opt_id
                  : Utils.GenUid();

            workspace.BlockDB.Add(ID, this);
            Workspace = workspace;

            OutputConnection = null;
            NextConnection = null;
            PreviousConnection = null;
            InputList = new List<Input>();

            workspace.AddTopBlock(this);
        }

        /// <summary>
        /// Clone a block from this block
        /// </summary>
        public Block Clone()
        {
            XmlNode xmlNode = Xml.BlockToDomWithXY(this, true);
            Block newBlock = Xml.DomToBlock(xmlNode, Workspace);
            Workspace.AddTopBlock(newBlock);
            return newBlock;
        }

        public void Dispose(bool healStack = false)
        {
            if (null == this.Workspace)
            {
                // Allready deleted.
                return;
            }
            
            UnPlug(healStack);
            
            // This block is now at the top of the workspace.
            // Remove this block from the workspace's list of top-most blocks.
            Workspace.RemoveTopBlock(this);
            // Remove from block database.
            Workspace.BlockDB.Remove(this.ID);
            Workspace = null;
            
            // Just deleting this block from the DOM would result in a memory leak as
            // well as corruption of the connectoin database. Therefore we must
            // methodically step through the blocks and carefully disassemble them.
            
            // First,dispose of all mychildren.
            for (int i = ChildBlocks.Count - 1; i >= 0; i--)
            {
                this.ChildBlocks[i].Dispose(false);
            }
            
            // Then dispose of myself.
            // Dispose of all inputs and theri fields.
            foreach (var input in InputList)
            {
                input.Dispose();
            }
    
            var connections = this.GetConnections();
            foreach (var connection in connections)
            {
                connection.Disconnect();
                connection.Dispose();
            }
        }

        /// <summary>
        /// Sets the mutator for this block.  Called from BlockFractory, and can only be called once (for now).
        /// </summary>
        public void SetMutator(Mutator mutator)
        {
            if (this.Mutator != null)
                throw new Exception("Cannot change mutators on a block.");
            this.Mutator = mutator;
            mutator.AttachToBlock(this);
        }
        
        /// <summary>
        /// updates the inputs and all connections with potentially new values,
        /// changing the shape of the block. This method should only be called by the constructor, or Mutators.
        /// </summary>
        public void Reshape(List<Input> newInputList, Connection updatedOutput, Connection updatedPrev, Connection updatedNext)
        {
            if (updatedOutput != null)
            {
                if (updatedPrev != null)
                    throw new Exception("A block cannot have both an output connection and a previous connection.");
                if (updatedOutput.Type != Define.EConnection.OutputValue)
                    throw new Exception("updatedOutput Connection type is not OUTPUT_VALUE");
            }
            if (updatedPrev != null && updatedPrev.Type != Define.EConnection.PrevStatement)
            {
                throw new Exception("updatedPrev Connection type is not PREVIOUS_STATEMENT");
            }
            if (updatedNext != null && updatedNext.Type != Define.EConnection.NextStatement)
            {
                throw new Exception("updatedNext Connection type is not CONNECTION_TYPE_NEXT");
            }

            bool updateInputs = false;
            bool updateConnection = false;
            
            //dispose old first
            List<Input> oldInputs = InputList;
            foreach (Input input in oldInputs)
            {
                if (!newInputList.Contains(input))
                {
                    input.Dispose();
                    updateInputs = true;
                }
            }
            foreach (Input input in newInputList)
            {
                if (!oldInputs.Contains(input))
                {
                    input.SourceBlock = this;
                    updateInputs = true;
                }
            }
            InputList = newInputList;

            updateConnection = OutputConnection != updatedOutput ||
                               PreviousConnection != updatedPrev ||
                               NextConnection != updatedNext;  

            if (updatedOutput != null)
                updatedOutput.SourceBlock = this;
            if (OutputConnection != null && OutputConnection != updatedOutput)
            {
                OutputConnection.Disconnect();
                OutputConnection.Dispose();
            }
            OutputConnection = updatedOutput;

            if (updatedPrev != null)
                updatedPrev.SourceBlock = this;
            if (PreviousConnection != null && PreviousConnection != updatedPrev)
            {
                PreviousConnection.Disconnect();
                PreviousConnection.Dispose();
            }
            PreviousConnection = updatedPrev;

            if (updatedNext != null)
                updatedNext.SourceBlock = this;
            if (NextConnection != null && NextConnection != updatedNext)
            {
                NextConnection.Disconnect();
                NextConnection.Dispose();
            }
            NextConnection = updatedNext;

            if (updateInputs && updateConnection) FireUpdate(1 << (int) UpdateState.Inputs | 1 << (int) UpdateState.Connections);
            else if (updateInputs)                FireUpdate(1 << (int) UpdateState.Inputs);
            else if (updateConnection)            FireUpdate(1 << (int) UpdateState.Connections);
        }

        /// <summary>
        /// updates the inputs
        /// </summary>
        /// <param name="newInputList"></param>
        public void Reshape(List<Input> newInputList)
        {
            Reshape(newInputList, OutputConnection, PreviousConnection, NextConnection);
        }

        /// <summary>
        /// Unplug this block from its superior block.  If this block is a statement,
        /// optionally reconnect the block underneath with the block on top.
        /// </summary>
        /// <param name="optHealStack">Disconnect child statement and reconnect stack</param>
        public void UnPlug(bool optHealStack = false)
        {
            if (this.OutputConnection != null)
            {
                if (this.OutputConnection.IsConnected)
                    this.OutputConnection.Disconnect();
            }
            else if (this.PreviousConnection != null)
            {
                Connection previousTarget = null;
                if (this.PreviousConnection.IsConnected)
                {
                    previousTarget = PreviousConnection.TargetConnection;
                    PreviousConnection.Disconnect();
                }
                Block nextBlock = this.NextBlock;
                if (optHealStack && nextBlock != null)
                {
                    var nextTarget = this.NextConnection.TargetConnection;
                    nextTarget.Disconnect();
                    if (previousTarget != null && previousTarget.CheckType(nextTarget))
                    {
                        previousTarget.Connect(nextTarget);
                    }
                }
            }
        }
    
        /// <summary>
        /// Return s all connections orgination from this block.
        /// </summary>
        public List<Connection> GetConnections()
        {
            var myConnections = new List<Connection>();
            if (null != OutputConnection)
            {
                myConnections.Add(OutputConnection);
            }
            if (null != PreviousConnection)
            {
                myConnections.Add(PreviousConnection);
            }
            if (null != NextConnection)
            {
                myConnections.Add(NextConnection);
            }
            
            for (int i = 0; i < InputList.Count; i++)
            {
                var input = InputList[i];
                if (null != input.Connection)
                {
                    myConnections.Add(input.Connection);
                }
            }
            
            return myConnections;
        }
    
        /// <summary>
        /// Walks down a stack of blocks and finds the last next connection on the stack.
        /// </summary>
        /// <returns> The last next connection on the stack,or null.</returns>
        public Connection LastConnectionInStack()
        {
            var nextConnection = NextConnection;
            while (null != nextConnection)
            {
                var nextBlock = nextConnection.TargetBlock;
                if (nextBlock == null)
                {
                    // Found a next connection with nothing on the other side.
                    return nextConnection;
                }
                nextConnection = nextBlock.NextConnection;
            }
            // Ran out of next connections.
            return null;
        }

        /// <summary>
        /// Get output, previous, next connection by connection type
        /// </summary>
        public Connection GetFirstClassConnection(Define.EConnection connectionType)
        {
            switch (connectionType)
            {
                case Define.EConnection.OutputValue: return OutputConnection;
                case Define.EConnection.PrevStatement: return PreviousConnection;
                case Define.EConnection.NextStatement: return NextConnection;
            }
            throw new Exception("Block GetFirstClassConnection: Only get output, previous, next connection");
        }

        /// <summary>
        /// add a new input
        /// </summary>
        /// <param name="index">insert the new input at the index</param>
        public void AppendInput(Input input, int index = -1)
        {
            if (!InputList.Contains(input))
            {
                input.SourceBlock = this;
                if (index > 0) InputList.Insert(index, input);
                else InputList.Add(input);

                FireUpdate(1 << (int) UpdateState.Inputs);
            }
        }

        /// <summary>
        /// Remove an input from this block.
        /// </summary>
        public void RemoveInput(Input input)
        {
            if (InputList.Contains(input))
            {
                input.Dispose();
                InputList.Remove(input);
                
                FireUpdate(1 << (int) UpdateState.Inputs);
            }
        }

        /// <summary>
        /// Check exist a named input object
        /// </summary>
        public bool HasInput(string name)
        {
            return InputList.Any(t => name.Equals(t.Name));
        }

        /// <summary>
        /// Fetches the named input object.
        /// </summary>
        public Input GetInput(string name)
        {
            for (int i = 0; i < InputList.Count; i++)
            {
                if (name.Equals(InputList[i].Name))
                    return InputList[i];
            }
            return null;
        }

        /// <summary>
        /// Return the input that connects to the specified block.
        /// @param {!Blockly.Block} block A block connected to an input on this block.
        /// @return {Blockly.Input} The input that connects to the specified block.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public Input GetInputWithBlock(Block block)
        {
            for (int i = 0; i < InputList.Count; i++)
            {
                var input = InputList[i];
                if (null != input.Connection && input.Connection.TargetBlock == block)
                {
                    return input;
                }
            }
            return null;
        }
    
        /// <summary>
        /// Fetches the block attached to the named input.
        /// </summary>
        /// <returns>The attached value block, or null if the input is either disconnected or if the input does not exist.</returns>
        public Block GetInputTargetBlock(string name)
        {
            Input input = this.GetInput(name);
            if (input != null && input.Connection != null && input.Connection.TargetBlock != null)
                return input.Connection.TargetBlock;
            return null;
        }
    
        /// <summary>
        /// Return the parent block that surrounds the current block,or null if this
        /// block has no surrounding block. A parent block might just be the previous
        /// statement,whereas the surrounding block is an if statement,while loop,etc.
        /// @return {Blockly.Block} The block that surrounds the current block.
        /// </summary>
        /// <returns></returns>
        public Block GetSurroundParent()
        {
            var block = this;
            var prevBlock = block;
            do
            {
                prevBlock = block;
                block = block.ParentBlock;
                if (null == block)
                {
                    // Ran off the top.
                    return null;
                }
            } while (block.NextBlock == prevBlock);
            // This block is an enclosing parent,not just a statement in a stack.
            return block;
        }
    
        /// <summary>
        /// Return the next statement block directly connected to this block.
        /// </summary>
        public Block NextBlock
        {
            get { return null != NextConnection ? NextConnection.TargetBlock : null; }
        }
    
        /// <summary>
        /// Return the top-most block in this block's tree.
        /// This will return itself if this block is at the top level.
        /// </summary>
        public Block RootBlock
        {
            get
            {
                Block rootBlock;
                var block = this;
                do
                {
                    rootBlock = block;
                    block = rootBlock.ParentBlock;
                } while (null != block);
                return rootBlock;
            }
        }
    
        public void SetParent(Block newParent)
        {
            if (newParent == ParentBlock)
            {
                return;
            }
            if (null != ParentBlock)
            {
                // Remove this block from the old parent's child list.
                ParentBlock.ChildBlocks.Remove(this);
    
                // Disconnect from superior blocks
                if (null != this.PreviousConnection && this.PreviousConnection.IsConnected)
                {
                    throw new Exception("Still connected to previous block.");
                }
                if (null != OutputConnection && this.OutputConnection.IsConnected)
                {
                    throw new Exception("Still connected to parent block.");
                }
                this.ParentBlock = null;
                // This block hasn't actually moved on-screen,so there's no need to update
                // its connection locations.
            }
            else
            {
                // Remove this block from the workspace's list of top-most blocks.
                this.Workspace.RemoveTopBlock(this);
            }

            this.ParentBlock = newParent;
            if (newParent != null)
                newParent.ChildBlocks.Add(this);
            else
                this.Workspace.AddTopBlock(this);
        }
        
        /// <summary>
        /// Find all the blocks that are directly nested inside this one.
        /// Includes value and block inputs,as well as any following statement.
        /// Excludes any connection on an outpu tab or any preceding statement.
        /// </summary>
        public List<Block> GetChildren()
        {
            return ChildBlocks;
        }
    
        /// <summary>
        /// Find all the blocks that are directly or indirectly nested inside this one.
        /// Includes this block in the list.
        /// Includes value and block inputs, as well as any following statements.
        /// Excludes any connection on an output tab or any preceding statements.
        /// </summary>
        public List<Block> GetDescendants()
        {
            var blocks = new List<Block> {this};
    
            for (int i = 0; i < ChildBlocks.Count; i++)
            {
                blocks.AddRange(ChildBlocks[i].GetDescendants());
            }
    
            return blocks;
        }

        /// <summary>
        /// Returns the named field from a block.
        /// </summary>
        public Field GetField(string name)
        {
            foreach (var input in InputList)
            {
                foreach (Field field in input.FieldRow)
                {
                    if (!string.IsNullOrEmpty(field.Name) && field.Name.Equals(name))
                        return field;
                }
            }
            return null;
        }
    
        /// <summary>
        /// Return all variables referenced by this block.
        /// </summary>
        /// <returns> List of variable names.</returns>
        public List<string> GetVars()
        {
            var vars = new List<string>();
            foreach (var input in InputList)
            {
                foreach (var field in input.FieldRow)
                {
                    if (field is FieldVariable)
                        vars.Add(field.GetValue());
                }
            }
            return vars;
        }
    
        /// <summary>
        /// Notification that a variable is renaming.
        /// If the name matches one of this block's variables,rename it.
        /// </summary>
        /// <param name="oldName"> Previous name of variable</param>
        /// <param name="newName"> Renamed Variable.</param>
        public void RenameVar(string oldName, string newName)
        {
            foreach (var input in InputList)
            {
                foreach (var field in input.FieldRow)
                {
                    if (field is FieldVariable && Names.Equals(oldName, field.GetValue()))
                    {
                        field.SetValue(newName);
                    }
                }
            }
        }
        
        /// <summary>
        /// Returns the langugage-neutral value from the field of a block.
        /// </summary>
        /// <param name="name"> The name of the field.</param>
        /// <returns>Value from the field or null if field does not exist.</returns>
        public string GetFieldValue(string name)
        {
            var field = this.GetField(name);
            if (null != field)
            {
                return field.GetValue();
            }
            return null;
        }
    
        /// <summary>
        /// Change the field value for a block (e.g. "CHOOSE" or "REMOVE").
        /// </summary>
        /// <param name="name"> The name of the field.</param>
        /// <param name="newValue"> Value to be the new field.</param>
        public void SetFieldValue(string name, string newValue)
        {
            var field = GetField(name);
            if (null == field) Debug.LogError("Field " + name + " not found");
            field.SetValue(newValue);
        }
        
        #region State Properties
        
        /// <summary>
        /// if this block is disabled
        /// </summary>
        private bool mDisabled = false;
        public bool Disabled
        {
            get { return mDisabled; }
            set
            {
                if (Disabled != value)
                {
                    mDisabled = value;
                    FireUpdate(1 << (int) UpdateState.IsDisabled);
                }
            }
        }

        /// <summary>
        /// whether this block is deletable or not.
        /// </summary>
        private bool mDeletable = true;
        public bool Deletable
        {
            get { return mDeletable && !mIsShadow && !(Workspace != null && Workspace.Options.ReadOnly); }
            set
            {
                if (mDeletable != value)
                {
                    mDeletable = value;
                    FireUpdate(1 << (int) UpdateState.IsDeletable);
                }
            }
        }

        /// <summary>
        /// whether this block is movable or not.
        /// </summary>
        private bool mMovable = true;
        public bool Movable
        {
            get { return mMovable && !mIsShadow && !(Workspace != null && Workspace.Options.ReadOnly); }
            set
            {
                if (mMovable != value)
                {
                    mMovable = value;
                    FireUpdate(1 << (int) UpdateState.IsMovable);
                }
            }
        }

        /// <summary>
        /// whether this block is a shadow block or not.
        /// </summary>
        private bool mIsShadow = false;
        public bool IsShadow
        {
            get { return mIsShadow; }
            set
            {
                if (mIsShadow != value)
                {
                    mIsShadow = value;
                    FireUpdate(1 << (int) UpdateState.IsShadow);
                }
            }
        }
    
        /// <summary>
        /// whether this block is editable or not.
        /// </summary>
        private bool mEditable = true;
        public bool Editable
        {
            get { return mEditable && !(Workspace != null && Workspace.Options.ReadOnly); }
            set
            {
                if (mEditable != value)
                {
                    mEditable = value;
                    FireUpdate(1 << (int) UpdateState.IsEditable);
                }
            }
        }
        
        /// <summary>
        /// Whether the block is collapsed.
        /// </summary>
        private bool mCollapsed = false;
        public bool Collapsed
        {
            get { return mCollapsed; }
            set
            {
                if (mCollapsed != value)
                {
                    mCollapsed = value;
                    FireUpdate(1 << (int) UpdateState.IsCollapsed);
                }
            }
        }

        /// <summary>
        /// -1: not defined; 0: defined false; 1: defined true
        /// </summary>
        private int mInputsInlineState = -1;

        /// <summary>
        /// Set whether value inputs are arranged horizontally or vertically.
        /// </summary>
        /// <param name="value"> Ture if inputs are horizontal.</param>
        public void SetInputsInline(bool value)
        {
            if (value && mInputsInlineState != 1)
            {
                mInputsInlineState = 1;
                FireUpdate(1 << (int) UpdateState.IsInputInline);
            }
            else if (!value && mInputsInlineState != 0)
            {
                mInputsInlineState = 0;
                FireUpdate(1 << (int) UpdateState.IsInputInline);
            }
        }

        /// <summary>
        /// Get whether value inputs are arranged horizontally or vertically.
        /// </summary>
        /// <returns> True if inputs are horizontal.</returns>
        public bool GetInputsInline()
        {
            if (mInputsInlineState >= 0)
            {
                // Set explicitly.
                return mInputsInlineState == 1;
            }

            // Not defined explicitly. Figure out what would look best.
            for (int i = 1; i < InputList.Count; i++)
            {
                if (InputList[i - 1].Type == Define.EConnection.DummyInput &&
                    InputList[i].Type == Define.EConnection.DummyInput)
                {
                    // Two dummy inputs in a row. Don't inline them.
                    return false;
                }
            }
            for (int i = 1; i < InputList.Count; i++)
            {
                if (InputList[i - 1].Type == Define.EConnection.InputValue &&
                    InputList[i].Type == Define.EConnection.DummyInput)
                {
                    // Dummy input after a value inpput . Inline them.
                    return true;
                }
            }
            return false;
        }
    
        /// <summary>
        /// Get whether the block is disabled or not due to parents.
        /// The block's own disabled property is not considered.
        /// </summary>
        /// <returns> True if disabled.</returns>
        public bool GetInheritedDisabled()
        {
            var ancestor = this.GetSurroundParent();
            while (null != ancestor)
            {
                if (ancestor.Disabled)
                {
                    return true;
                }
                ancestor = ancestor.GetSurroundParent();
            }
            // Ran off the top.
            return false;
        }
        
        /// <summary>
        /// Recursively checks whether all statement and value inputs are filled with
        /// blocks. Also checks all following statement blocks in this stack.
        /// </summary>
        public bool AllInputsFilled(bool optShadowBlocksAreFilled = true)
        {
            // Account for the shadow block filledness toggle.
            if (!optShadowBlocksAreFilled && mIsShadow)
            {
                return false;
            }
    
            // Recursively check each input block of the current block.
            for (int i = 0; i < InputList.Count; i++)
            {
                var input = InputList[i];
                if (null == input.Connection)
                {
                    continue;
                }
                var target = input.Connection.TargetBlock;
                if (null != target || !target.AllInputsFilled(optShadowBlocksAreFilled))
                {
                    return false;
                }
            }
    
            // Recusively check the next block after the current block.
            var next = this.NextBlock;
            if (null != next)
            {
                return next.AllInputsFilled(optShadowBlocksAreFilled);
            }
            return true;
        }
        
        #endregion
        
        /// <summary>
        /// This method returns a string describing this Block in developer terms (type
        /// name and ID; English only).
        /// 
        /// Intended to on be used in console logs and errors. If you need a string that
        /// uses the user's native language (including block text, field values, and
        /// child blocks), use [toString()]{@link Blockly.Block#toString}.
        /// </summary>
        /// <returns></returns>
        public string ToDevString()
        {
            var msg = !string.IsNullOrEmpty(this.Type) ? "\"" + this.Type + "\" block" : "Block";
            if (!string.IsNullOrEmpty(this.ID))
            {
                msg += " (id=\"" + this.ID + "\")";
            }
            return msg;
        }
    }
}   
