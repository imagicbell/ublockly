/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Functions for generating c# code for blocks.
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
