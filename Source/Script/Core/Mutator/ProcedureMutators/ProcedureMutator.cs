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
using System.Xml;

namespace UBlockly
{
    public abstract class ProcedureMutator : Mutator
    {
        protected Procedure mProcedure;
        public Procedure ProcedureInfo { get { return mProcedure; } }

        public ProcedureDB ProcedureDB
        {
            get { return mBlock != null ? mBlock.Workspace.ProcedureDB : null; }
        }

        /// <summary>
        /// The procedure name associated with this mutator. May be null if not attached to a block.
        /// </summary>
        public string GetProcedureName()
        {
            return mProcedure == null ? null : mProcedure.Name;
        }

        /// <summary>
        /// Sets the procedure name for this mutator (and thus mBlock) when it is not on the workspace, 
        /// </summary>
        /// <param name="name"></param>
        public void SetProcedureName(string name)
        {
            if (mBlock != null && ProcedureDB.ContainsDefinition(GetProcedureName()))
                throw new Exception("Can't rename procedure when it's on the workspace, please use Procedures.MutateProcedure!");
            SetProcedureNameInternal(name);
        }

        /// <summary>
        /// The list of argument names.
        /// </summary>
        public List<string> GetArgumentNameList()
        {
            return mProcedure == null ? null : mProcedure.Arguments;
        }

        /// <summary>
        /// Whether the function is allow to have a statement body
        /// </summary>
        public bool HasStatement()
        {
            return mProcedure != null && mProcedure.DefinitionHasStatementBody;
        }
        
        public void Mutate(Procedure info)
        {
            if (mProcedure == info)
                return;

            mProcedure = info;
            if (mBlock != null)
            {
                UpdateInternal();
            }
        }

        protected override void OnAttached()
        {
            UpdateInternal();
        }
        
        public sealed override XmlElement ToXml()
        {
            return mProcedure != null ? SerializeProcedure(mProcedure) : null;
        }

        public sealed override void FromXml(XmlElement xmlElement)
        {
            mProcedure = DeserializeProcedure(xmlElement);
            UpdateInternal();
        }

        /// <summary>
        /// Applies the mutation to mBlock.
        /// </summary>
        protected virtual void UpdateInternal()
        {
            if (mProcedure != null)
                mBlock.Reshape(BuildUpdatedInputs());
        }
        
        protected abstract void SetProcedureNameInternal(string name);
        protected abstract List<Input> BuildUpdatedInputs();
        protected abstract XmlElement SerializeProcedure(Procedure info);
        protected abstract Procedure DeserializeProcedure(XmlElement xmlElement);
    }
}
