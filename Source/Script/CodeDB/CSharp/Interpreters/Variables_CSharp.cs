/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Functions for interpreting c# code for blocks.
****************************************************************************/


using System.Collections;

namespace UBlockly
{
    [CodeInterpreter(BlockType = "variables_get")]
    public class Variables_Get_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            string varName = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            return CSharp.VariableDatas.GetData(varName);
        }
    }

    [CodeInterpreter(BlockType = "variables_set")]
    public class Variables_Set_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string varName = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE");
            yield return ctor;
            CSharp.VariableDatas.SetData(varName, ctor.Data);
        }
    }

    [CodeInterpreter(BlockType = "variables_change")]
    public class Math_Change_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string tmp = block.GetFieldValue("VAR");

            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "DELTA", new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;

            Number result = CSharp.VariableDatas.GetData(tmp).NumberValue + arg1.NumberValue;
            CSharp.VariableDatas.SetData(tmp, new DataStruct(result));
        }
    }
}
