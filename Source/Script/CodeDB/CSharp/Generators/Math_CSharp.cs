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

namespace UBlockly
{
    public partial class CSharpGenerator
    {
        [CodeGenerator(BlockType = "math_number")]
        private CodeStruct Math_Number(Block block)
        {
            string code = block.GetFieldValue("NUM");
            float num = float.Parse(code);
            int order = num < 0 ? CSharp.ORDER_UNARY : CSharp.ORDER_ATOMIC;
            return new CodeStruct(code, order);
        }

        [CodeGenerator(BlockType = "math_arithmetic")]
        private CodeStruct Math_Arithmetic(Block block)
        {
            Dictionary<string, KeyValuePair<string, int>> operators = new Dictionary<string, KeyValuePair<string, int>>
            {
                {"ADD", new KeyValuePair<string, int>(" + ", CSharp.ORDER_ADDITIVE)},
                {"MINUS", new KeyValuePair<string, int>(" - ", CSharp.ORDER_ADDITIVE)},
                {"MULTIPLY", new KeyValuePair<string, int>(" * ", CSharp.ORDER_MULTIPLICATIVE)},
                {"DIVIDE", new KeyValuePair<string, int>(" / ", CSharp.ORDER_MULTIPLICATIVE)},
                {"POWER", new KeyValuePair<string, int>(null, CSharp.ORDER_COMMA)},
            };
            
            var pair = operators[block.GetFieldValue("OP")];
            string op = pair.Key;
            int order = pair.Value;
            string arg0 = CSharp.Generator.ValueToCode(block, "A", order, "0");
            string arg1 = CSharp.Generator.ValueToCode(block, "B", order, "0");
            string code;
            if (op == null)
            {
                //Power in c# requires a special case since it has no operator.
                code = string.Format("System.Math.Pow({0}, {1})", arg0, arg1);
                return new CodeStruct(code, CSharp.ORDER_EXPRESSION);
            }
            code = arg0 + op + arg1;
            return new CodeStruct(code, order);
        }
    }
}
