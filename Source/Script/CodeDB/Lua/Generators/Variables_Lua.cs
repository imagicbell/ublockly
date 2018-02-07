/****************************************************************************

Functions for generating lua code for blocks.

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

namespace UBlockly
{
    public partial class LuaGenerator
    {
        [CodeGenerator(BlockType = "variables_get")]
        private CodeStruct Variables_Get(Block block)
        {
            string code = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            return new CodeStruct(code, Lua.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "variables_set")]
        private string Variables_Set(Block block)
        {
            var arg = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_NONE, "0");
            var varName = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            return varName + " = " + arg + "\n";
        }
        
        [CodeGenerator(BlockType = "variables_change")]
        private string Math_Change(Block block)
        {
            string varName = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Variables.NAME_TYPE);
            string arg = Lua.Generator.ValueToCode(block, "DELTA", Lua.ORDER_ADDITIVE, "0");
            return string.Format("{0} = {0} + {1}\n", varName, arg);
        }
    }
}
