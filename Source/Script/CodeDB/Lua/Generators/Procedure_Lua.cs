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

using System.Collections.Generic;
using System.Text;

namespace UBlockly
{
    public partial class LuaGenerator
    {
        [CodeGenerator(BlockType = "procedures_defreturn")]
        private object Procedure_DefReturn(Block block)
        {
            string funcName = Lua.VariableNames.GetName(block.GetFieldValue("NAME"), Define.PROCEDURE_CATEGORY_NAME);
            string branch = Lua.Generator.StatementToCode(block, "STACK", "");
            string returnValue = Lua.Generator.ValueToCode(block, "RETURN", Lua.ORDER_NONE);
            if (!string.IsNullOrEmpty(returnValue))
                returnValue = "    return " + returnValue + "\n";

            StringBuilder argumentSB = new StringBuilder(); 
            List<string> arguments = ProcedureDB.GetProcedureArguments(block);
            for (int i = 0; i < arguments.Count; i++)
            {
                argumentSB.Append(Lua.VariableNames.GetName(arguments[i], Define.VARIABLE_CATEGORY_NAME));
                if (i < arguments.Count - 1)
                    argumentSB.Append(", ");
            }

            string code = string.Format("function {0}({1})\n{2}{3}end\n", funcName, argumentSB.ToString(), branch, returnValue);
            code = Lua.Generator.Scrub(block, code);
            
            // Add % so as not to collide with helper functions in definitions list.
            Lua.Generator.AddFunction('%' + funcName, code);
            return null;
        }

        [CodeGenerator(BlockType = "procedures_defnoreturn")]
        private object Procedure_DefNoReturn(Block block)
        {
            return Procedure_DefReturn(block);
        }

        [CodeGenerator(BlockType = "procedures_callreturn")]
        private CodeStruct Procedure_CallReturn(Block block)
        {
            // Call a procedure with a return value.
            string funcName = Lua.VariableNames.GetName(block.GetFieldValue("NAME"), Define.PROCEDURE_CATEGORY_NAME);
            StringBuilder argumentSB = new StringBuilder();
            List<string> arguments = ProcedureDB.GetProcedureArguments(block);
            for (int i = 0; i < arguments.Count; i++)
            {
                argumentSB.Append(Lua.Generator.ValueToCode(block, "ARG" + i, Lua.ORDER_NONE, "nil"));
                if (i < arguments.Count - 1)
                    argumentSB.Append(", ");
            }

            string code = string.Format("{0}({1})", funcName, argumentSB.ToString());
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }

        [CodeGenerator(BlockType = "procedures_callnoreturn")]
        private string Procedure_CallNoReturn(Block block)
        {
            return Procedure_CallReturn(block).code + "\n";
        }

        [CodeGenerator(BlockType = "procedures_ifreturn")]
        private string Proceudre_IfReturn(Block block)
        {
            // Conditionally return value from a procedure.
            string condition = Lua.Generator.ValueToCode(block, "CONDITION", Lua.ORDER_NONE, "false");
            string code = string.Format("if {0} then\n", condition);
            string value = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_NONE, "nil");
            code += "  return " + value + '\n';
            code += "end\n";
            return code;
        }
    }
}
