/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Xml;

namespace PTGame.Blockly
{
    public class ProcedureCallMutator : ProcedureMutator
    {
        /// <summary>
        /// This retrieves the block's Input that represents the nth argument.
        /// </summary>
        /// <param name="index">The index of the argument asked for.</param>
        public Input GetArgumenInput(int index)
        {
            return index <= mBlock.InputList.Count ? mBlock.InputList[index + 1] : null;
        }
        
        protected override void SetProcedureNameInternal(string name)
        {
            mProcedure = mProcedure.CloneWithName(name);
            mBlock.GetField(ProcedureDB.PROCEDURE_NAME_FIELD).SetText(name);
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();
            if (mProcedure != null)
            {
                mBlock.GetField(ProcedureDB.PROCEDURE_NAME_FIELD).SetText(mProcedure.Name);
            }
        }

        /// <summary>
        /// A new set of Inputs reflecting the current Procedure state.
        /// </summary>
        protected override List<Input> BuildUpdatedInputs()
        {
            List<string> args = mProcedure.Arguments;
            int argCount = args.Count;
            List<Input> inputs = new List<Input>();
            
            // Procedure name
            inputs.Add(mBlock.InputList[0]);  
            
            // Argument inputs
            for (int i = 0; i < argCount; ++i)
            {
                Input stackInput = InputFactory.Create(Blockly.INPUT_VALUE, "ARG" + i, Blockly.ALIGH_RIGHT, null);
                
                // add "with: " label
                if (i == 0)
                {
                    FieldLabel withLabel = new FieldLabel("WITH", Blockly.Msg[MsgDefine.PROCEDURES_CALL_BEFORE_PARAMS]);
                    stackInput.AppendField(withLabel);
                }
                
                // add argument's label
                FieldLabel label = new FieldLabel(null, args[i]);
                stackInput.AppendField(label);
                
                inputs.Add(stackInput);
            }
            return inputs;
        }

        protected override XmlElement SerializeProcedure(Procedure info)
        {
            return Procedure.Serialize(info, false);
        }

        protected override Procedure DeserializeProcedure(XmlElement xmlElement)
        {
            Procedure info = Procedure.Deserialize(xmlElement);
            if (string.IsNullOrEmpty(info.Name))
                throw new Exception("No procedure name specified in mutation for " + mBlock.ToDevString());
            return info;
        }
    }
}