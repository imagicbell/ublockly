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

namespace UBlockly
{
    public class Input
    {
        public string Name { get; private set; }
        public readonly Define.EConnection Type;
        public readonly Connection Connection;
        public readonly List<Field> FieldRow;
        
        /// <summary>
        /// block that is the parent of this input.
        /// </summary>
        private Block mSourceBlock;

        public Block SourceBlock
        {
            get { return mSourceBlock; }
            set
            {
                if (mSourceBlock == value) return;
                if (mSourceBlock != null && value != null)
                    throw new Exception("Input is already a member of another block.");
                mSourceBlock = value;
                if (Connection != null)
                    Connection.SourceBlock = value;
                foreach (Field field in FieldRow)
                {
                    field.SetSourceBlock(value);
                }
            }
        }

        /// <summary>
        /// block that is connected to this input
        /// </summary>
        public Block ConnectedBlock
        {
            get { return Connection != null ? Connection.TargetBlock : null; }
        }

        /// <summary>
        /// Class for an input with an optional field.
        /// </summary>
        /// <param name="type"> The type of the input.</param>
        /// <param name="name"> Language-neutral identifier which may used to find this input again</param>
        /// <param name="block"> The block containing this input.</param>
        /// <param name="connection"> Optional connection for this input</param>
        public Input(Define.EConnection type, string name, Block block, Connection connection = null)
        {
            if (type != Define.EConnection.DummyInput && string.IsNullOrEmpty(name))
            {
                throw new Exception("Value inputs and statement inputs must have non-empty name.");
            }
            Type = type;
            Name = name;

            mSourceBlock = block;
            Connection = connection;

            FieldRow = new List<Field>();

            Align = Define.EAlign.Left;
        }

        /// <summary>
        /// Class for an input with an optional field.
        /// </summary>
        public Input(Define.EConnection type, string name, Connection connection = null) : this(type, name, null, connection)
        {
        }

        public void SetName(string newName)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(newName))
                Name = newName;
        }

        /// <summary>
        /// Alignment of input's fields (left,right or center).
        /// </summary>
        public Define.EAlign Align { get; private set; }

        /// <summary>
        /// Change the alignment of the connection's field(s).
        /// </summary>
        public Input SetAlign(Define.EAlign align)
        {
            if (this.Align != align)
            {
                this.Align = align;
            }
            return this;
        }

        /// <summary>
        /// Add a field and all prefix and suffix fields, to the end of the input's field row.
        /// </summary>
        /// <param name="field"> Something to add as a field</param>
        public Input AppendField(Field field)
        {
            this.InsertFieldAt(FieldRow.Count, field);
            return this;
        }

        /// <summary>
        /// Add a label from string, to the end of the input's field row.
        /// </summary>
        /// <param name="field"> label string to add as a field</param>
        /// <param name="optName"> Language-neutral identifier which may used to find this field again. Should be unique to the host block.</param>
        /// <returns> The input being append to (to allow chaining).</returns>
        public Input AppendField(string field, string optName = null)
        {
            this.InsertFieldAt(FieldRow.Count, field, optName);
            return this;
        }

        /// <summary>
        /// Inserts a label from string
        /// </summary>
        /// <param name="index"> The index at which to insert field.</param>
        /// <param name="field"> label string to add as a field.</param>
        /// <param name="optName"> Language-neutral identifier which may used to find this field again. Should be unique to the host block.</param>
        public int InsertFieldAt(int index, string field, string optName = null)
        {
            FieldLabel fieldLabel = string.IsNullOrEmpty(field) ? null : new FieldLabel(optName, field);
            return InsertFieldAt(index, fieldLabel);
        }

        /// <summary>
        /// Inserts a field ,and all prefix and suffix fields, at the location of the input's field row.
        /// </summary>
        /// <param name="index"> The index at which to insert field.</param>
        /// <param name="field"> Something to add as a field.</param>
        public int InsertFieldAt(int index, Field field)
        {
            if (index < 0 || index > FieldRow.Count)
                throw new Exception("index " + index + " out of bounds.");
            
            field.SetSourceBlock(this.mSourceBlock);
            if (field.PrefixField != null)
            {
                // Add any prefix.
                index = this.InsertFieldAt(index, field.PrefixField);
            }
            
            // Add the field to the field row.
            this.FieldRow.Insert(index, field);
            ++index;

            if (field.SuffixField != null)
            {
                // Add any suffix
                index = this.InsertFieldAt(index, field.SuffixField);
            }
            return index;
        }

        /// <summary>
        /// Remove a field from this input.
        /// </summary>
        /// <param name="fieldName">the name of the field</param>
        public void RemoveField(string fieldName)
        {
            foreach (Field field in FieldRow)
            {
                if (field.Name.Equals(fieldName))
                {
                    field.Dispose();
                    FieldRow.Remove(field);
                }
            }
        }
        
        /// <summary>
        /// Change a connection's compatibility.
        /// </summary>
        public void SetCheck(string check)
        {
            if (string.IsNullOrEmpty(check))
                return;
            if (this.Connection == null)
                throw new Exception("This input does not have a connection.");
            this.Connection.SetCheck(new List<string>() {check});
        }

        /// <summary>
        /// Change a connection's compatibility.
        /// </summary>
        public void SetCheck(List<string> check)
        {
            if (check == null || check.Count == 0)
                return;
            if (this.Connection == null)
                throw new Exception("This input does not have a connection.");
            this.Connection.SetCheck(check);
        }
        
        public void Dispose()
        {
            foreach (var field in FieldRow)
            {
                field.Dispose();
            }

            if (Connection != null)
            {
                Connection.Disconnect();
                Connection.Dispose();
            }

            this.mSourceBlock = null;
        }
    }
}
