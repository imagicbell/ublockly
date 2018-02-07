/****************************************************************************

Functions for generating c# code for blocks.

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

namespace UBlockly
{
    public partial class CSharpGenerator 
    {
        [CodeGenerator(BlockType = "variables_get")]
        private CodeStruct Variables_Get(Block block)
        {
            string code = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            return new CodeStruct(code, CSharp.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "variables_set")]
        private string Variables_Set(Block block)
        {
            string varName = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            string arg = CSharp.Generator.ValueToCode(block, "VALUE", CSharp.ORDER_NONE, "0");
            return varName + " = " + arg + ";\n";
        }
    }
}
