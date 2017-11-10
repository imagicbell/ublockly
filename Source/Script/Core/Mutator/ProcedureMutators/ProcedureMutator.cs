/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
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

        public override bool NeedEditor
        {
            get { return true; }
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
