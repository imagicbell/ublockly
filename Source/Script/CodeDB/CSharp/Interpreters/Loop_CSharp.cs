/****************************************************************************

Functions for interpreting c# code for blocks.

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
using System.Collections;

namespace UBlockly
{
    public abstract class ControlCmdtor : EnumeratorCmdtor
    {
        protected ControlFlowType mFlowState;

        public bool NeedBreak
        {
            get { return mFlowState == ControlFlowType.Break; }
        }

        public bool NeedContinue
        {
            get { return mFlowState == ControlFlowType.Continue; }
        }

        public void SetFlowState(ControlFlowType flowState)
        {
            mFlowState = flowState;
        }

        public void ResetFlowState()
        {
            mFlowState = ControlFlowType.None;
        }

        /// <summary>
        /// find the parent loop block of the flow block
        /// </summary>
        public static ControlCmdtor FindParentControlCmdtor(Block block)
        {
            Block loopBlock = block;
            while (loopBlock != null && !IsLoopBlock(loopBlock))
            {
                loopBlock = loopBlock.ParentBlock;
            }

            if (loopBlock == null) return null;
            return CSharp.Interpreter.GetBlockInterpreter(loopBlock) as ControlCmdtor;
        }

        /// <summary>
        /// check if the block is skipped running by the control flow: break, continue..
        /// </summary>
        public static bool SkipRunByControlFlow(Block block)
        {
            ControlCmdtor loopCmdtor = FindParentControlCmdtor(block);
            if (loopCmdtor != null)
                return loopCmdtor.NeedBreak || loopCmdtor.NeedContinue;
            return false;
        }
        
        public static bool IsLoopBlock(Block block)
        {
            return block.Type.Equals("controls_repeat") ||
                   block.Type.Equals("controls_repeat_ext") ||
                   block.Type.Equals("controls_whileUntil") ||
                   block.Type.Equals("controls_for") ||
                   block.Type.Equals("controls_forEach");
        }
    }
    
    [CodeInterpreter(BlockType = "controls_repeat")]
    public class Control_Repeat_Cmdtor : ControlCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            ResetFlowState();
            
            int repeats = int.Parse(block.GetFieldValue("TIMES"));
            for (int i = 0; i < repeats; i++)
            {
                yield return CSharp.Interpreter.StatementRun(block, "DO");
                
                //reset flow control
                if (NeedBreak) break;
                if (NeedContinue) ResetFlowState();
            }
        }
    }

    [CodeInterpreter(BlockType = "controls_repeat_ext")]
    public class Control_RepeatExt_Cmdtor : ControlCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            ResetFlowState();
            
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TIMES", new DataStruct(0));
            yield return ctor;
            DataStruct repeats = ctor.Data;
            
            if (repeats.Type != Define.EDataType.Number)
                throw new Exception("input value \"TIMES\" of block controls_repeat_ext must be a number type");
            int repeatsInt = (int) repeats.NumberValue.Value;
            for (int i = 0; i < repeatsInt; i++)
            {
                yield return CSharp.Interpreter.StatementRun(block, "DO");
                
                //reset flow control
                if (NeedBreak) break;
                if (NeedContinue) ResetFlowState();
            }
        }
    }

    [CodeInterpreter(BlockType = "controls_whileUntil")]
    public class Control_WhileUntil_Cmdtor : ControlCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            ResetFlowState();
            
            bool until = block.GetFieldValue("MODE").Equals("UNTIL");
            
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "BOOL", new DataStruct(false));
            yield return ctor;
            DataStruct arg = ctor.Data;
            
            if (arg.Type != Define.EDataType.Boolean)
                throw new Exception("input value \"BOOL\" of block controls_whileUntil must be a boolean type");

            bool condition = until ? !arg.BooleanValue : arg.BooleanValue;
            while (condition)
            {
                yield return CSharp.Interpreter.StatementRun(block, "DO");
                
                ctor = CSharp.Interpreter.ValueReturn(block, "BOOL", new DataStruct(false));
                yield return ctor;
                arg = ctor.Data;
                condition = until ? !arg.BooleanValue : arg.BooleanValue;
                
                //reset flow control
                if (NeedBreak) break;
                if (NeedContinue) ResetFlowState();
            }
        }
    }

    [CodeInterpreter(BlockType = "controls_for")]
    public class Control_For_Cmdtor : ControlCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            ResetFlowState();
            
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "FROM", new DataStruct(0));
            yield return ctor;
            DataStruct from = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "TO", new DataStruct(0));
            yield return ctor;
            DataStruct to = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "BY", new DataStruct(0));
            yield return ctor;
            DataStruct increment = ctor.Data;
                
            if (!from.IsNumber || !to.IsNumber|| !increment.IsNumber)
                throw new Exception("input value \"FROM\", \"TO\", \"BY\" of block controls_for must be number type");

            for (float i = from.NumberValue.Value; i <= to.NumberValue.Value; i += increment.NumberValue.Value)
            {
                yield return CSharp.Interpreter.StatementRun(block, "DO");
                
                //reset flow control
                if (NeedBreak) break;
                if (NeedContinue) ResetFlowState();
            }
        }
    }

    [CodeInterpreter(BlockType = "controls_forEach")]
    public class Control_ForEach_Cmdtor : ControlCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            ResetFlowState();
            
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "LIST");
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            if (arg0.IsUndefined) 
                arg0 = new DataStruct(new ArrayList());
            if (!arg0.IsList)
                throw new Exception("input value \"LIST\" of block controls_forEach must be LIST type");

            string variable0 = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Variables.NAME_TYPE);
            foreach (var e  in arg0.ListValue)
            {
                DataStruct data;
                if (e is bool) data = new DataStruct((bool) e);
                else if (e is float) data = new DataStruct(new Number((float) e));
                else if (e is string) data = new DataStruct((string) e);
                else if (e is ArrayList) data = new DataStruct((ArrayList) e);
                else throw new Exception("LIST element is undefined type.");

                CSharp.VariableDatas.SetData(variable0, data);
                yield return CSharp.Interpreter.StatementRun(block, "DO");
                
                //reset flow control
                if (NeedBreak) break;
                if (NeedContinue) ResetFlowState();
            }
        }
    }

    [CodeInterpreter(BlockType = "controls_flow_statements")]
    public class Control_FlowStatement_Cmdtor : VoidCmdtor
    {
        protected override void Execute(Block block)
        {
            ControlCmdtor loopCmdtor = ControlCmdtor.FindParentControlCmdtor(block);
            if (loopCmdtor == null)
                throw new Exception("blocks of \"break\" and \"continue\" can only be put in blocks of loop control types");
            
            switch (block.GetFieldValue("FLOW"))
            {
                case "BREAK":
                    loopCmdtor.SetFlowState(ControlFlowType.Break); 
                    return;
                    
                case "CONTINUE":
                    loopCmdtor.SetFlowState(ControlFlowType.Continue); 
                    return;
            }
        }
    }
}
