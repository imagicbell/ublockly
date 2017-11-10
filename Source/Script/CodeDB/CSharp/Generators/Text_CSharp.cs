/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Functions for generating c# code for blocks.
****************************************************************************/

namespace UBlockly
{
    public partial class CSharpGenerator
    {
        [CodeGenerator(BlockType = "text")]
        private CodeStruct Text(Block block)
        {
            string code = CSharp.Generator.Quote(block.GetFieldValue("TEXT"));
            return new CodeStruct(code, CSharp.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "text_print")]
        private string Text_Print(Block block)
        {
            string text = CSharp.Generator.ValueToCode(block, "TEXT", CSharp.ORDER_NONE, "\'\'");
            return string.Format("UnityEngine.Debug.Log({0});\n", text);
        }
    }
}
