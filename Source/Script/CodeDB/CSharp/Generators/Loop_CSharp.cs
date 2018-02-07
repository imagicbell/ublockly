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

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace UBlockly
{
    public partial class CSharpGenerator 
    {
        [CodeGenerator(BlockType = "controls_repeat")]
        private string Control_Repeat(Block block)
        {
            int repeats = int.Parse(block.GetFieldValue("TIMES"));
            string branch = CSharp.Generator.StatementToCode(block, "DO", "");
            string loopVar = CSharp.VariableNames.GetDistinctName("count");
            string code = string.Format("for (int {0} = 0; {0} < {1}; ++{0})\n{{\n    {2}\n}}\n", loopVar, repeats, branch);
            return code;
        }
        
        [CodeGenerator(BlockType = "controls_repeat_ext")]
        private string Control_RepeatExt(Block block)
        {
            string repeats = CSharp.Generator.ValueToCode(block, "TIMES", CSharp.ORDER_NONE, "0");
            int repeatsInt = 0;
            if (int.TryParse(repeats, out repeatsInt))
                repeats = repeatsInt.ToString();
            else
                repeats = string.Format("(int)({0})", repeats);
            string branch = CSharp.Generator.StatementToCode(block, "DO", "");
            string loopVar = CSharp.VariableNames.GetDistinctName("count");
            string code = string.Format("for (int {0} = 0; {0} < {1}; ++{0})\n{{\n    {2}\n}}\n", loopVar, repeats, branch);
            return code;
            
        }
        
        [CodeGenerator(BlockType = "controls_whileUntil")]
        private string Control_WhileUntil(Block block)
        {
            bool until = block.GetFieldValue("MODE").Equals("UNTIL");
            string arg0 = CSharp.Generator.ValueToCode(block, "BOOL",
                until ? Lua.ORDER_UNARY : Lua.ORDER_NONE, "false");
            string branch = CSharp.Generator.StatementToCode(block, "DO", "");
            branch = CSharp.Generator.AddLoopTrap(branch, block.ID);
            if (until)
            {
                arg0 = "!" + arg0;
            }
            return string.Format("while ({0})\n{{\n    {1}}}\n", arg0, branch);
        }
        
        [CodeGenerator(BlockType = "controls_for")]
        private string Control_For(Block block)
        {
            string variable0 = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Variables.NAME_TYPE);
            string from = CSharp.Generator.ValueToCode(block, "FROM", CSharp.ORDER_NONE, "0");
            string to = CSharp.Generator.ValueToCode(block, "TO", CSharp.ORDER_NONE, "0");
            string increment = CSharp.Generator.ValueToCode(block, "BY", CSharp.ORDER_NONE, "1");
            string branch = CSharp.Generator.StatementToCode(block, "DO", "");
            branch = CSharp.Generator.AddLoopTrap(branch, block.ID);

            string incValue;
            float fromValue, toValue, incrementVar;
            if (float.TryParse(from, out fromValue) && float.TryParse(to, out toValue) && float.TryParse(increment, out incrementVar))
            {
                bool up = fromValue <= toValue;
                float step = Math.Abs(incrementVar);
                incValue = up ? "+= " + step : "-= " + step;
                string code = string.Format("for (float {0} = {1}, {0} {2} {3}, {0} {4})\n{{\n    {5}\n}}\n",
                                            variable0, from, up ? "<=" : ">=", to, incValue, branch);
                return code;
            }
            else
            {
                throw new Exception("input value \"FROM\" \"TO\" \"BY\" of block controls_for must be a number type.");
            }
        }
        
        [CodeGenerator(BlockType = "controls_forEach")]
        private string Control_ForEach(Block block)
        {
            string variable0 = CSharp.VariableNames.GetName(block.GetFieldValue("VAR"), Variables.NAME_TYPE);
            string arg0 = CSharp.Generator.ValueToCode(block, "LIST", CSharp.ORDER_NONE, "new System.Collections.Generic.List<float>()");
            string branch = CSharp.Generator.StatementToCode(block, "DO", "\n");

            StringBuilder code = new StringBuilder();
            
            string listVar = arg0;
            Regex regex = new Regex(@"^\w+$");
            if (!regex.IsMatch(arg0))
            {
                listVar = CSharp.VariableNames.GetDistinctName(variable0 + "_list");
                code.Append(string.Format("var {0} = {1};\n", listVar, arg0));
            }

            code.Append(string.Format("foreach (var {0} in {1})\n{{\n    {2}\n}}\n", variable0, listVar, branch));
            return code.ToString();
        }
        
        [CodeGenerator(BlockType = "controls_flow_statements")]
        private string Control_FlowStatement(Block block)
        {
            switch (block.GetFieldValue("FLOW"))
            {
                case "BREAK": return "break;\n";
                case "CONTINUE": return "continue;\n";
            }
            throw new Exception("Unknown flow statement.");
        }
    }
} 
