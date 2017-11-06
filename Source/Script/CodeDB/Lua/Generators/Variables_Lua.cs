/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Functions for generating lua code for blocks.
****************************************************************************/

using System;

namespace PTGame.Blockly
{
    public partial class LuaGenerator
    {
        [CodeGenerator(BlockType = "variables_get")]
        private CodeStruct Variables_Get(Block block)
        {
            string code = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Blockly.VARIABLE_CATEGORY_NAME);
            return new CodeStruct(code, Lua.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "variables_set")]
        private string Variables_Set(Block block)
        {
            var arg = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_NONE, "0");
            var varName = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Blockly.VARIABLE_CATEGORY_NAME);
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