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

namespace UBlockly
{
    public partial class LuaGenerator
    {
        [CodeGenerator(BlockType = "controls_if")]
        private string Controls_If(Block block)
        {
            int n = 0;
            string code = "";
            string branchCode = null;
            string conditionCode = null;

            do
            {
                conditionCode = Lua.Generator.ValueToCode(block, "IF" + n, Lua.ORDER_NONE, "false");
                branchCode = Lua.Generator.StatementToCode(block, "DO" + n);

                code += (n > 0 ? "else" : "") +
                        "if " + conditionCode + " then\n" + branchCode;

                ++n;

            } while (block.GetInput("IF" + n) != null);

            if (block.GetInput("ELSE") != null)
            {
                branchCode = Lua.Generator.StatementToCode(block, "ELSE");
                code += "else\n" + branchCode;
            }

            return code + "end\n";
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
                {"NEQ", "~="},
                {"LT", "<"},
                {"LTE", "<="},
                {"GT", ">"},
                {"GTE", ">="}
            };

            string op = operators[block.GetFieldValue("OP")];
            string argument0 = Lua.Generator.ValueToCode(block, "A", Lua.ORDER_RELATIONAL, "0");
            string argument1 = Lua.Generator.ValueToCode(block, "B", Lua.ORDER_RELATIONAL, "0");

            string code = argument0 + " " + op + " " + argument1;
            return new CodeStruct(code, Lua.ORDER_RELATIONAL);
        }

        [CodeGenerator(BlockType = "logic_operation")]
        private CodeStruct Logic_Operation(Block block)
        {
            // Operations 'and', 'or'.
            bool isAnd = block.GetFieldValue("OP").Equals("AND");
            
            string op = isAnd ? "and" : "or";
            int order = isAnd ? Lua.ORDER_AND : Lua.ORDER_OR;

            string argument0 = Lua.Generator.ValueToCode(block, "A", order);
            string argument1 = Lua.Generator.ValueToCode(block, "B", order);
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
            string argument0 = Lua.Generator.ValueToCode(block, "BOOL", Lua.ORDER_UNARY, "true");
            string code = "not " + argument0;
            return new CodeStruct(code, Lua.ORDER_UNARY);
        }

        [CodeGenerator(BlockType = "logic_boolean")]
        private CodeStruct Logic_Boolean(Block block)
        {
            // Boolean values true and false.
            string code = block.GetFieldValue("BOOL").Equals("TRUE") ? "true" : "false";
            return new CodeStruct(code, Lua.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "logic_null")]
        private CodeStruct Logic_Null(Block block)
        {
            return new CodeStruct("nil", Lua.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "logic_ternary")]
        private CodeStruct Logic_Ternary(Block block)
        {
            string value_if = Lua.Generator.ValueToCode(block, "IF", Lua.ORDER_AND, "false");
            string value_then = Lua.Generator.ValueToCode(block, "THEN", Lua.ORDER_AND, "nil");
            string value_else = Lua.Generator.ValueToCode(block, "ELSE", Lua.ORDER_OR, "nil");

            string code = value_if + " and " + value_then + " or " + value_else;
            return new CodeStruct(code, Lua.ORDER_OR);
        }
    }
}
