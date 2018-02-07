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
using System.Text.RegularExpressions;

namespace UBlockly
{
    public partial class LuaGenerator
    {
        [CodeGenerator(BlockType = "text")]
        private CodeStruct Text(Block block)
        {
            string code = Lua.Generator.Quote(block.GetFieldValue("TEXT"));
            return new CodeStruct(code, Lua.ORDER_ATOMIC);
        }
        
        [CodeGenerator(BlockType = "text_join")]
        private CodeStruct Text_Join(Block block)
        {
            ItemListMutator mutator = block.Mutator as ItemListMutator;
            if (mutator == null)
                throw new Exception("Block \"text_join\" must have a mutater \"text_join_mutator\"");
            
            // Create a string made up of any number of elements of any type.
            if (mutator.ItemCount == 0)
            {
                return new CodeStruct("\'\'", Lua.ORDER_ATOMIC);
            }
            else if (mutator.ItemCount == 1)
            {
                string element = Lua.Generator.ValueToCode(block, "ADD0", Lua.ORDER_NONE);
                if (string.IsNullOrEmpty(element))
                    element = "\'\'";
                string code = "tostring(" + element + ")";
                return new CodeStruct(code, Lua.ORDER_HIGH);
            }
            else if (mutator.ItemCount == 2)
            {
                string element0 = Lua.Generator.ValueToCode(block, "ADD0", Lua.ORDER_CONCATENATION);
                if (string.IsNullOrEmpty(element0)) element0 = "\'\'";
                string element1 = Lua.Generator.ValueToCode(block, "ADD1", Lua.ORDER_CONCATENATION);
                if (string.IsNullOrEmpty(element1)) element1 = "\'\'";
                string code = element0 + " .. " + element1;
                return new CodeStruct(code, Lua.ORDER_CONCATENATION);
            }
            else
            {
                string[] elements = new string[mutator.ItemCount];
                for (int i = 0; i < mutator.ItemCount; i++)
                {
                    elements[i] = Lua.Generator.ValueToCode(block, "ADD" + i, Lua.ORDER_NONE);
                    if (string.IsNullOrEmpty(elements[i])) elements[i] = "\'\'";
                }
                string code = "table.concat({" + string.Join(", ", elements) + "})";
                return new CodeStruct(code, Lua.ORDER_HIGH);
            }
        }
        
        [CodeGenerator(BlockType = "text_append")]
        private string Text_Append(Block block)
        {
            // Append to a variable in place.
            string varName = Lua.VariableNames.GetName(block.GetFieldValue("VAR"), Define.VARIABLE_CATEGORY_NAME);
            string value = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_CONCATENATION);
            if (string.IsNullOrEmpty(value)) value = "\'\'";
            return varName + " = " + varName + " .. " + value + "\n";
        }
        
        [CodeGenerator(BlockType = "text_length")]
        private CodeStruct Text_Length(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_UNARY);
            if (string.IsNullOrEmpty(text)) text = "\'\'";
            return new CodeStruct("#" + text, Lua.ORDER_UNARY);
        }

        [CodeGenerator(BlockType = "text_isEmpty")]
        private CodeStruct Text_IsEmpty(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_UNARY);
            if (string.IsNullOrEmpty(text)) text = "\'\'";
            return new CodeStruct("#" + text + " == 0", Lua.ORDER_RELATIONAL);
        }

        [CodeGenerator(BlockType = "text_indexOf")]
        private CodeStruct Text_IndexOf(Block block)
        {
            // Search the text for a substring.
            string substring = Lua.Generator.ValueToCode(block, "FIND", Lua.ORDER_NONE);
            if (string.IsNullOrEmpty(substring)) substring = "\'\'";

            string text = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_NONE);
            if (string.IsNullOrEmpty(text)) text = "\'\'";

            string functionName;
            if (block.GetFieldValue("END").Equals("FIRST"))
            {
                functionName = Lua.Generator.ProvideFunction("firstIndexOf",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(str, substr) 
                        local i = string.find(str, substr, 1, true)
                        if i == nil then
                            return 0
                        else
                            return i
                        end
                    end");
            }
            else
            {
                functionName = Lua.Generator.ProvideFunction("lastIndexOf",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(str, substr) 
                        local i = string.find(string.reverse(str), string.reverse(substr), 1, true)
                        if i == nil then
                            return 0
                        else
                            return #str + 2 - i - #substr
                        end
                    end");
            }
            string code = functionName + "(" + text + ", " + substring + ")";
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_charAt")]
        private CodeStruct Text_CharAt(Block block)
        {
            // Get letter at index.
            string where = block.GetFieldValue("WHERE");
            if (string.IsNullOrEmpty(where)) where = "FROM_START";
            int atOrder = where == "FROM_END" ? Lua.ORDER_UNARY : Lua.ORDER_NONE;
            string at = Lua.Generator.ValueToCode(block, "AT", atOrder, "1");
            string text = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_NONE, "\'\'");

            StringBuilder code = new StringBuilder();
            if (where == "RANDOM")
            {
                string funcName = Lua.Generator.ProvideFunction("text_random_letter",
"function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(str)
    local index = math.random(string.len(str))
    return string.sub(str, index, index)
end");
                code.Append(funcName + "(" + text + ")");
            }
            else
            {
                string start = null;
                if (where == "FIRST")
                    start = "1";
                else if (where == "LAST")
                    start = "-1";
                else if (where == "FROM_START")
                    start = at;
                else if (where == "FROM_END")
                    start = "-" + at;

                Regex regex = new Regex(@"^-?\w*$");
                if (regex.IsMatch(start))
                {
                    code.Append(string.Format("string.sub({0}, {1}, {1})", text, start));
                }
                else
                {
                    string funcName = Lua.Generator.ProvideFunction("text_char_at",
"function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(str, index)
    return string.sub(str, index, index)
end");
                    code.Append(string.Format("{0}({1}, {2})", funcName, text, start));
                }
            }
            return new CodeStruct(code.ToString(), Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_getSubstring")]
        private CodeStruct Text_GetSubstring(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "STRING", Lua.ORDER_NONE, "\'\'");

            //get start index
            string where1 = block.GetFieldValue("WHERE1");
            if (string.IsNullOrEmpty(where1)) where1 = "FROM_START";
            int at1Order = where1 == "FROM_END" ? Lua.ORDER_UNARY : Lua.ORDER_NONE;
            string at1 = Lua.Generator.ValueToCode(block, "AT1", at1Order, "1");
            string start = null;
            if (where1 == "FIRST")
                start = "1";
            else if (where1 == "FROM_START")
                start = at1;
            else if (where1 == "FROM_END")
                start = "-" + at1;
            
            //get end index
            string where2 = block.GetFieldValue("WHERE2");
            if (string.IsNullOrEmpty(where2)) where2 = "FROM_START";
            int at2Order = where2 == "FROM_END" ? Lua.ORDER_UNARY : Lua.ORDER_NONE;
            string at2 = Lua.Generator.ValueToCode(block, "AT2", at2Order, "1");
            string end = null;
            if (where2 == "LAST")
                end = "-1";
            else if (where2 == "FROM_START")
                end = at2;
            else if (where2 == "FROM_END")
                end = "-" + at2;
            
            string code = string.Format("string.sub({0}, {1}, {2})", text, start, end);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_changeCase")]
        private CodeStruct Text_ChangeCase(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_NONE, "\'\'");
            string op = block.GetFieldValue("CASE");
            string funcName = null;
            if (op.Equals("UPPERCASE"))
            {
                funcName = "string.upper";
            }
            else if (op.Equals("LOWERCASE"))
            {
                funcName = "string.lower";
            }
            else if (op.Equals("TITLECASE"))
            {
                funcName = Lua.Generator.ProvideFunction("text_titlecase",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(str)
                        local buf = {}
                        local inWord = false
                        for i = 1, #str do
                            local c = string.sub(str, i, i)
                            if inWord then
                                table.insert(buf, string.lower(c))
                                if string.find(c, ""%s"") then
                                    inWord = false
                                end
                            else
                                table.insert(buf, string.upper(c))
                                inWord = true
                            end   
                        end 
                        return table.concat(buf)
                    end");
            }

            string code = string.Format("{0}({1})", funcName, text);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_trim")]
        private CodeStruct Text_Trim(Block block)
        {
            Dictionary<string, string> OPERATORS = new Dictionary<string, string>
            {
                {"LEFT", @"^%s*(,-)"},
                {"RIGHT", @"(.-)%s*$"},
                {"BOTH", @"^%s*(.-)%s*$"}
            };
            string op = OPERATORS[block.GetFieldValue("MODE")];
            string text = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_NONE, "\'\'");
            string code = string.Format(@"string.gsub({0}, ""{1}"", ""%1"")", text, op);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_print")]
        private string Text_Print(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_NONE, "\'\'");
            return string.Format("print({0})\n", text);
        }
        
        /*[CodeGenerator(BlockType = "text_prompt_ext")]
        private CodeStruct Text_PromptExt(Block block)
        {
            
        }
        
        [CodeGenerator(BlockType = "text_prompt")]
        private CodeStruct Text_Prompt(Block block)
        {
            
        }*/
        
        [CodeGenerator(BlockType = "text_count")]
        private CodeStruct Text_Count(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_NONE, "\'\'");
            string sub = Lua.Generator.ValueToCode(block, "SUB", Lua.ORDER_NONE, "\'\'");
            string funcName = Lua.Generator.ProvideFunction("text_count",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(haystack, needle)
                    if #needle == 0 then return #haystack + 1 end
                    local i = 1
                    local count = 0
                    while true do
                        i = string.find(haystack, needle, i, true)
                        if i == nil then break end
                        count = count + 1
                        i = i + #needle
                    end    
                    return count
                end");
            string code = string.Format("{0}({1}, {2})", funcName, text, sub);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_replace")]
        private CodeStruct Text_Replace(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_NONE, "\'\'");
            string from = Lua.Generator.ValueToCode(block, "FROM", Lua.ORDER_NONE, "\'\'");
            string to = Lua.Generator.ValueToCode(block, "TO", Lua.ORDER_NONE, "\'\'");
            string funcName = Lua.Generator.ProvideFunction("text_replace",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(haystack, needle, replacement)
                    local buf = {}
                    local i = 1
                    while i <= #haystack do
                        if string.sub(haystack, i, i + #needle - 1) == needle then
                            for j = 1, #replacement do
                                table.insert(buf, string.sub(replacement, j, j))
                            end
                            i = i + #needle
                        else
                            table.insert(buf, string.sub(haystack, i, i))
                            i = i + 1    
                        end
                    end
                    return table.concat(buf)
                end");
            string code = string.Format("{0}({1}, {2}, {3})", funcName, text, from, to);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "text_reverse")]
        private CodeStruct Text_Reverse(Block block)
        {
            string text = Lua.Generator.ValueToCode(block, "TEXT", Lua.ORDER_HIGH, "\'\'");
            string code = string.Format("string.reverse({0})", text);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
    }
}
