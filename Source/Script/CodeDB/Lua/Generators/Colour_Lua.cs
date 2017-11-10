/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Functions for generating lua code for blocks.
****************************************************************************/

using System;
using System.Collections.Generic;

namespace UBlockly
{
    public partial class LuaGenerator
    {
        [CodeGenerator(BlockType = "colour_picker")]
        private CodeStruct Colour_Picker(Block block)
        {
            string code = string.Format("\'{0}\'", block.GetFieldValue("COLOUR"));
            return new CodeStruct(code, Lua.ORDER_ATOMIC);
        }
        
        [CodeGenerator(BlockType = "colour_random")]
        private CodeStruct Colour_Random(Block block)
        {
            string code = @"string.format(""#%06x"", math.random(0, 2^24 - 1))";
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "colour_rgb")]
        private CodeStruct Colour_RGB(Block block)
        {
            float red = float.Parse(Lua.Generator.ValueToCode(block, "RED", Lua.ORDER_NONE, "0"));
            float green = float.Parse(Lua.Generator.ValueToCode(block, "GREEN", Lua.ORDER_NONE, "0"));
            float blue = float.Parse(Lua.Generator.ValueToCode(block, "BLUE", Lua.ORDER_NONE, "0"));
            string funcName = Lua.Generator.ProvideFunction("colour_rgb",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(r, g, b)
                     r = math.floor(math.min(100, math.max(0, r)) * 2.55 + .5)
                     g = math.floor(math.min(100, math.max(0, g)) * 2.55 + .5)
                     b = math.floor(math.min(100, math.max(0, b)) * 2.55 + .5)
                     return string.format(""#%02x%02x%02x"", r, g, b)                     
                end");
            string code = string.Format("{0}({1}, {2}, {3})", funcName, red, green, blue);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "colour_blend")]
        private CodeStruct Colour_Blend(Block block)
        {
            string color1 = Lua.Generator.ValueToCode(block, "COLOUR1", Lua.ORDER_NONE, "\'#000000\'");
            string color2 = Lua.Generator.ValueToCode(block, "COLOUR2", Lua.ORDER_NONE, "\'#000000\'");
            string ratio = Lua.Generator.ValueToCode(block, "RATIO", Lua.ORDER_NONE, "0");
            string funcName = Lua.Generator.ProvideFunction("colour_blend",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(colour1, colour2, ratio)
                     local r1 = tonumber(string.sub(colour1, 2, 3), 16)
                     local r2 = tonumber(string.sub(colour2, 2, 3), 16)
                     local g1 = tonumber(string.sub(colour1, 4, 5), 16)
                     local g2 = tonumber(string.sub(colour2, 4, 5), 16)
                     local b1 = tonumber(string.sub(colour1, 6, 7), 16)
                     local b2 = tonumber(string.sub(colour2, 6, 7), 16)
                     local ratio = math.min(1, math.max(0, ratio))
                     local r = math.floor(r1 * (1 - ratio) + r2 * ratio + .5)
                     local g = math.floor(g1 * (1 - ratio) + g2 * ratio + .5)
                     local b = math.floor(b1 * (1 - ratio) + b2 * ratio + .5)
                     return string.format(""#%02x%02x%02x"", r, g, b)                    
                end");
            string code = string.Format("{0}({1}, {2}, {3})", funcName, color1, color2, ratio);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
    }
}
