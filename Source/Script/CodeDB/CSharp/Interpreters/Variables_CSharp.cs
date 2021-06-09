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
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE");
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

            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "DELTA", new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;

            Number result = CSharp.VariableDatas.GetData(tmp).NumberValue + arg1.NumberValue;
            CSharp.VariableDatas.SetData(tmp, new DataStruct(result));
        }
    }
}
