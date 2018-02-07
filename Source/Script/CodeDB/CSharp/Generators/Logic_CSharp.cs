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

using System.Collections.Generic;
using System.Text;

namespace UBlockly
{
    public partial class CSharpGenerator 
    {
        [CodeGenerator(BlockType = "controls_if")]
        private string Controls_If(Block block)
        {
            int n = 0;
            StringBuilder code = new StringBuilder();
            string branchCode = null;
            string conditionCode = null;

            do
            {
                conditionCode = CSharp.Generator.ValueToCode(block, "IF" + n, CSharp.ORDER_NONE, "false");
                branchCode = CSharp.Generator.StatementToCode(block, "DO" + n);

                if (n == 0)
                {
                    code.Append(string.Format("if ({0})\n{{\n    {1}}}\n", conditionCode, branchCode));
                }
                else
                {
                    code.Append(string.Format("else if ({0})\n{{\n    {1}}}\n", conditionCode, branchCode));
                }
                ++n;

            } while (block.GetInput("IF" + n) != null);

            if (block.GetInput("ELSE") != null)
            {
                branchCode = CSharp.Generator.StatementToCode(block, "ELSE");
                code.Append(string.Format("else\n{{\n    {0}}}\n", branchCode));
            }

            return code.ToString();
        }

        [CodeGenerator(BlockType = "controls_ifelse")]
        private string Controls_IfElse(Block block)
        {
            return Controls_If(block);
        }

        [CodeGenerator(BlockType = "logic_compare")]
        private CodeStruct Logic_Compare(Block block)
        {
            Dictionary<string, string> operators = new Dictionary<string, string>()
            {
                {"EQ", "=="},
                {"NEQ", "!="},
                {"LT", "<"},
                {"LTE", "<="},
                {"GT", ">"},
                {"GTE", ">="}
            };

            string op = operators[block.GetFieldValue("OP")];
            string argument0 = CSharp.Generator.ValueToCode(block, "A", CSharp.ORDER_RELATIONAL, "0");
            string argument1 = CSharp.Generator.ValueToCode(block, "B", CSharp.ORDER_RELATIONAL, "0");

            string code = argument0 + " " + op + " " + argument1;
            return new CodeStruct(code, CSharp.ORDER_RELATIONAL);
        }

        [CodeGenerator(BlockType = "logic_operation")]
        private CodeStruct Logic_Operation(Block block)
        {
            // Operations 'and', 'or'.
            bool isAnd = block.GetFieldValue("OP").Equals("AND");
            
            string op = isAnd ? "&&" : "||";
            int order = isAnd ? CSharp.ORDER_LOGICAL_AND : CSharp.ORDER_LOGICAL_OR;

            string argument0 = CSharp.Generator.ValueToCode(block, "A", order);
            string argument1 = CSharp.Generator.ValueToCode(block, "B", order);
            if (string.IsNullOrEmpty(argument0) && string.IsNullOrEmpty(argument1))
            {
                argument0 = "false";
                argument1 = "false";
            }
            else
            {
                //Single missing arguments have no effect on the return value.
                string defaultArgument = isAnd ? "true" : "false";
                if (string.IsNullOrEmpty(argument0))
                    argument0 = defaultArgument;
                if (string.IsNullOrEmpty(argument1))
                    argument1 = defaultArgument;
            }

            string code = argument0 + " " + op + " " + argument1;
            return new CodeStruct(code, order);
        }

        [CodeGenerator(BlockType = "logic_negate")]
        private CodeStruct Logic_Negate(Block block)
        {
            string argument0 = CSharp.Generator.ValueToCode(block, "BOOL", CSharp.ORDER_UNARY, "true");
            string code = "!" + argument0;
            return new CodeStruct(code, CSharp.ORDER_UNARY);
        }

        [CodeGenerator(BlockType = "logic_boolean")]
        private CodeStruct Logic_Boolean(Block block)
        {
            // Boolean values true and false.
            string code = block.GetFieldValue("BOOL").Equals("TRUE") ? "true" : "false";
            return new CodeStruct(code, CSharp.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "logic_null")]
        private CodeStruct Logic_Null(Block block)
        {
            //actually there's no logic_null in c#
            return new CodeStruct("false", CSharp.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "logic_ternary")]
        private CodeStruct Logic_Ternary(Block block)
        {
            string value_if = CSharp.Generator.ValueToCode(block, "IF", CSharp.ORDER_LOGICAL_AND, "false");
            string value_then = CSharp.Generator.ValueToCode(block, "THEN", CSharp.ORDER_LOGICAL_AND, "nil");
            string value_else = CSharp.Generator.ValueToCode(block, "ELSE", CSharp.ORDER_LOGICAL_OR, "nil");

            string code = string.Format("{0} ? {1} : {2}", value_if, value_then, value_else);
            return new CodeStruct(code, CSharp.ORDER_LOGICAL_OR);
        }
    }
}
