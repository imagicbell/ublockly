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
    [CodeInterpreter(BlockType = "controls_if")]
    public class Controls_If_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            int n = 0;
            bool satisfyIf = false;
            do
            {
                CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "IF" + n);
                yield return ctor;
                DataStruct condition = ctor.Data;
                if (!condition.IsUndefined && condition.IsBoolean && condition.BooleanValue)
                {
                    yield return CSharp.Interpreter.StatementRun(block, "DO" + n);
                    satisfyIf = true;
                    break;
                }
                ++n;
            } while (block.GetInput("IF" + n) != null);
            
            if (!satisfyIf && block.GetInput("ELSE") != null)
            {
                yield return CSharp.Interpreter.StatementRun(block, "ELSE");
            }
        }
    }

    [CodeInterpreter(BlockType = "controls_ifelse")]
    public class Controls_IfElse_Cmdtor : Controls_If_Cmdtor
    {
    }

    [CodeInterpreter(BlockType = "logic_compare")]
    public class Logic_Compare_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string op = block.GetFieldValue("OP");

            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "A", new DataStruct(0));
            yield return ctor;
            DataStruct argument0 = ctor.Data;

            ctor = CSharp.Interpreter.ValueReturn(block, "B", new DataStruct(0));
            yield return ctor;
            DataStruct argument1 = ctor.Data;
            
            if (argument0.Type != argument1.Type)
                throw new Exception("arguments of block logic_compare should be the same data type");
            
            DataStruct returnData = new DataStruct(false);
            switch (op)
            {
                case "EQ":
                    returnData.BooleanValue = argument0 == argument1;
                    break;
                    
                case "NEQ":
                    returnData.BooleanValue = argument0 != argument1;
                    break;
                    
                case "LT":
                    if (argument0.Type != Define.EDataType.Number)
                        throw new Exception("block logic_compare's \"<\" can't compare two strings and booleans");
                    returnData.BooleanValue = argument0.NumberValue < argument1.NumberValue;
                    break;
                    
                case "LTE":
                    if (argument0.Type != Define.EDataType.Number)
                        throw new Exception("block logic_compare's \"<=\" can't compare two strings and booleans");
                    returnData.BooleanValue = argument0.NumberValue <= argument1.NumberValue;
                    break;
                    
                case "GT":
                    if (argument0.Type != Define.EDataType.Number)
                        throw new Exception("block logic_compare's \">\" can't compare two strings and booleans");
                    returnData.BooleanValue = argument0.NumberValue > argument1.NumberValue;
                    break;
                    
                case "GTE":
                    if (argument0.Type != Define.EDataType.Number)
                        throw new Exception("block logic_compare's \">=\" can't compare two strings and booleans");
                    returnData.BooleanValue = argument0.NumberValue >= argument1.NumberValue;
                    break;
            }
            ReturnData(returnData);
        }
    }

    [CodeInterpreter(BlockType = "logic_operation")]
    public class Logic_Operation_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string op = block.GetFieldValue("OP");

            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "A", new DataStruct(false));
            yield return ctor;
            DataStruct argument0 = ctor.Data;

            ctor = CSharp.Interpreter.ValueReturn(block, "B", new DataStruct(false));
            yield return ctor;
            DataStruct argument1 = ctor.Data;
            
            if (argument0.Type != argument1.Type || argument0.Type != Define.EDataType.Boolean)
                throw new Exception("arguments of block logic_operation should be the same BOOLEAN type");
            
            DataStruct returnData = new DataStruct(false);
            switch (op)
            {
                case "AND":
                    returnData.BooleanValue = argument0.BooleanValue && argument1.BooleanValue;
                    break;
                case "OR":
                    returnData.BooleanValue = argument0.BooleanValue || argument1.BooleanValue;
                    break;
            }
            ReturnData(returnData);
        }
    }

    [CodeInterpreter(BlockType = "logic_negate")]
    public class Logic_Negate_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "BOOL", new DataStruct(false));
            yield return ctor;
            DataStruct argument = ctor.Data;
            
            if (argument.Type != Define.EDataType.Boolean)
                throw new Exception("argument of block logic_negate should be the BOOLEAN type");
            
            ReturnData(new DataStruct(!argument.BooleanValue));
        }
    }

    [CodeInterpreter(BlockType = "logic_boolean")]
    public class Logic_Boolean_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            string op = block.GetFieldValue("BOOL");
            switch (op)
            {
                case "TRUE": return new DataStruct(true);
                case "FALSE": return new DataStruct(false);
            }
            return new DataStruct(false);
        }
    }

    [CodeInterpreter(BlockType = "logic_null")]
    public class Logic_Null_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            return new DataStruct(false);
        }
    }

    [CodeInterpreter(BlockType = "logic_ternary")]
    public class Logic_Ternary_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "IF", new DataStruct(false));
            yield return ctor;
            DataStruct condition = ctor.Data;
            
            if (condition.Type != Define.EDataType.Boolean)
                throw new Exception("argument \"IF\" of block logic_ternary should be the BOOLEAN type");

            if (condition.BooleanValue)
            {
                yield return CSharp.Interpreter.StatementRun(block, "THEN");
            }
            else
            {
                yield return CSharp.Interpreter.StatementRun(block, "ELSE");
            }
        }
    }

    [CodeInterpreter(BlockType = "logic_toggle_boolean")]
    public class Logic_Toggle_Boolean_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            string toggleString = block.GetFieldValue("CHECKBOX");
            bool toggleValue = toggleString.Equals("TRUE");
            return new DataStruct(toggleValue);
        }
    }
}
