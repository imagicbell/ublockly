/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Functions for generating c# code for blocks.
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
