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
using System.Collections.Generic;
using System.Text;

namespace UBlockly
{
    public partial class LuaGenerator
    {
        /// <summary>
        /// If the loop body contains a "goto continue" statement, add a continue label
        /// to the loop body. Slightly inefficient, as continue labels will be generated
        /// in all outer loops, but this is safer than duplicating the logic of blockToCode.
        /// </summary>
        string AddContinueLabel(string branch)
        {
            if (branch.IndexOf("goto continue", StringComparison.Ordinal) > -1)
                return branch + "\t::continue::\n";
            return branch;
        }
        
        [CodeGenerator(BlockType = "controls_repeat")]
        private string Control_Repeat(Block block)
        {
            int repeats = int.Parse(block.GetFieldValue("TIMES"));
            string branch = Lua.Generator.StatementToCode(block, "DO", "");
            branch = AddContinueLabel(branch);
            string loopVar = Lua.VariableNames.GetDistinctName("count");
            string code = string.Format("for {0} = 1, {1} do\n{2}end\n", loopVar, repeats, branch);
            return code;
        }
        
        [CodeGenerator(BlockType = "controls_repeat_ext")]
        private string Control_RepeatExt(Block block)
        {
            string repeats = Lua.Generator.ValueToCode(block, "TIMES", Lua.ORDER_NONE, "0");
            int repeatsInt = 0;
            if (int.TryParse(repeats, out repeatsInt))
                repeats = repeatsInt.ToString();
            else
                repeats = string.Format("math.floor({0})", repeats);
            string branch = Lua.Generator.StatementToCode(block, "DO", "");
            branch = AddContinueLabel(branch);
            string loopVar = Lua.VariableNames.GetDistinctName("count");
            string code = string.Format("for {0} = 1, {1} do\n{2}end\n", loopVar, repeats, branch);
            return code;
        }
        
        [CodeGenerator(BlockType = "controls_whileUntil")]
        private string Control_WhileUntil(Block block)
        {
            bool until = block.GetFieldValue("MODE").Equals("UNTIL");
            string arg0 = Lua.Generator.ValueToCode(block, "BOOL",
                until ? Lua.ORDER_UNARY : Lua.ORDER_NONE, "false");
            string branch = Lua.Generator.StatementToCode(block, "DO", "");
            branch = Lua.Generator.AddLoopTrap(branch, block.ID);
            branch = AddContinueLabel(branch);
            if (until)
            {
                arg0 = "not " + arg0;
            }
            return string.Format("while {0} do\n{1}end\n", arg0, branch);
        }
        
        [CodeGenerator(BlockType = "controls_for")]
        private string Control_For(Block block)
        {
            string variable0 = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Variables.NAME_TYPE);
            string from = Lua.Generator.ValueToCode(block, "FROM", Lua.ORDER_NONE, "0");
            string to = Lua.Generator.ValueToCode(block, "TO", Lua.ORDER_NONE, "0");
            string increment = Lua.Generator.ValueToCode(block, "BY", Lua.ORDER_NONE, "1");
            string branch = Lua.Generator.StatementToCode(block, "DO", "");
            branch = Lua.Generator.AddLoopTrap(branch, block.ID);
            branch = AddContinueLabel(branch);

            StringBuilder code = new StringBuilder();
            string incValue;
            float fromValue, toValue, incrementVar;
            if (float.TryParse(from, out fromValue) && float.TryParse(to, out toValue) && float.TryParse(increment, out incrementVar))
            {
                bool up = fromValue <= toValue;
                float step = Math.Abs(incrementVar);
                incValue = (up ? "" : "-") + step;
            }
            else
            {
                incValue = Lua.VariableNames.GetDistinctName(variable0 + "_inc");
                code.Append(incValue + " = ");
                if (float.TryParse(increment, out incrementVar))
                    code.Append(Math.Abs(incrementVar) + "\n");
                else
                    code.Append(string.Format("math.abs({0})\n", increment));
                code.Append(string.Format("if ({0}) > ({1}) then\n", from, to));
                code.Append(string.Format("\t{0} = -{1}\n", incValue, incValue));
                code.Append("end\n");
            }
            code.Append(string.Format("for {0} = {1}, {2}, {3} do\n{4}end\n", variable0, from, to, incValue, branch));
            return code.ToString();
        }
        
        [CodeGenerator(BlockType = "controls_forEach")]
        private string Control_ForEach(Block block)
        {
            string variable0 = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Variables.NAME_TYPE);
            string arg0 = Lua.Generator.ValueToCode(block, "LIST", Lua.ORDER_NONE, "{}");
            string branch = Lua.Generator.StatementToCode(block, "DO", "\n");
            branch = AddContinueLabel(branch);
            string code = string.Format("for _, {0} in ipairs({1}) do \n {2}end\n", variable0, arg0, branch);
            return code;
        }
        
        [CodeGenerator(BlockType = "controls_flow_statements")]
        private string Control_FlowStatement(Block block)
        {
            switch (block.GetFieldValue("FLOW"))
            {
                case "BREAK": return "break\n";
                case "CONTINUE": return "goto continue\n";
            }
            throw new Exception("Unknown flow statement.");
        }
    }
}
