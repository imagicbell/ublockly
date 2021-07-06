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
    [MutatorClass(MutatorId = "procedures_callnoreturn_mutator;procedures_callreturn_mutator")]
    public class ProcedureCallMutator : ProcedureMutator
    {
        public override bool NeedEditor
        {
            get { return false; }
        }
        
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
                Input stackInput = InputFactory.Create(Define.EConnection.InputValue, "ARG" + i, Define.EAlign.Right, null);
                
                // add "with: " label
                if (i == 0)
                {
                    FieldLabel withLabel = new FieldLabel("WITH", I18n.Get(MsgDefine.PROCEDURES_CALL_BEFORE_PARAMS));
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
