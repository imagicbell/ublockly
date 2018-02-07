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
        [CodeGenerator(BlockType = "lists_create_empty")]
        private CodeStruct List_CreateEmpty(Block block)
        {
            return new CodeStruct("{}", Lua.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "lists_create_with")]
        private CodeStruct List_CreateWith(Block block)
        {
            ItemListMutator mutator = block.Mutator as ItemListMutator;
            if (mutator == null)
                throw new Exception("Block \"lists_create_with\" must have a mutator \"lists_create_with_item_mutator\"");
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mutator.ItemCount; i++)
            {
                sb.Append(Lua.Generator.ValueToCode(block, "ADD" + i, Lua.ORDER_NONE, "None"));
                if (i < mutator.ItemCount - 1)
                    sb.Append(", ");
            }
            string code = "{" + sb.ToString() + "}";
            return new CodeStruct(code, Lua.ORDER_ATOMIC);
        }

        [CodeGenerator(BlockType = "lists_repeat")]
        private CodeStruct List_Repeat(Block block)
        {
            string funcName = Lua.Generator.ProvideFunction("create_list_repeated",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(item, count)
                    local t = {}
                    for i = 1, count do
                       table.insert(t, item)
                    end
                    return t
                end");
            string element = Lua.Generator.ValueToCode(block, "ITEM", Lua.ORDER_NONE, "None");
            string repeatCount = Lua.Generator.ValueToCode(block, "NUM", Lua.ORDER_NONE, "0");
            string code = string.Format("{0}({1}, {2})", funcName, element, repeatCount);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "lists_reverse")]
        private CodeStruct List_Reverse(Block block)
        {
            string list = Lua.Generator.ValueToCode(block, "LIST", Lua.ORDER_NONE, "{}");
            string funcName = Lua.Generator.ProvideFunction("list_reverse",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(input)
                    local reversed = {}
                    for i = #input, 1, -1 do
                        table.insert(reversed, input[i])
                    end
                    return reversed
                end");
            string code = string.Format("{0}({1})", funcName, list);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "lists_isEmpty")]
        private CodeStruct List_IsEmpty(Block block)
        {
            string list = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_UNARY, "{}");
            string code = string.Format("#{0} == 0", list);
            return new CodeStruct(code, Lua.ORDER_RELATIONAL);
        }
        
        [CodeGenerator(BlockType = "lists_length")]
        private CodeStruct List_Length(Block block)
        {
            string list = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_UNARY, "{}");
            return new CodeStruct("#" + list, Lua.ORDER_UNARY);
        }
        
        [CodeGenerator(BlockType = "lists_indexOf")]
        private CodeStruct List_IndexOf(Block block)
        {
            string list = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_UNARY, "{}");
            string item = Lua.Generator.ValueToCode(block, "FIND", Lua.ORDER_NONE, "\'\'");
            string funcName;
            if (block.GetFieldValue("END").Equals("FIRST"))
            {
                funcName = Lua.Generator.ProvideFunction("first_index",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t, elem)
                        for k, v in ipairs(t) do
                            if v == elem then return k end
                        end
                        return 0
                    end");
            }
            else
            {
                funcName = Lua.Generator.ProvideFunction("last_index",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t, elem)
                        for i = #t, 1, -1 do
                            if t[i] == elem then return i end
                        end
                        return 0
                    end");
            }
            string code = string.Format("{0}({1}, {2})", funcName, list, item);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }

        private string GetListIndexStr(string listName, string where, string opt_at)
        {
            switch (where)
            {
                case "FIRST": return "1";
                case "FROM_END": return string.Format("#{0} + 1 - {1}", listName, opt_at);
                case "LAST": return "#" + listName;
                case "RANDOM": return "math.random(#" + listName + ")";
                default: return opt_at;
            }
        }

        [CodeGenerator(BlockType = "lists_getIndex")]
        private CodeStruct List_GetIndex(Block block)
        {
            string mode = block.GetFieldValue("MODE");
            if (string.IsNullOrEmpty(mode)) mode = "GET";

            string where = block.GetFieldValue("WHERE");
            if (string.IsNullOrEmpty(where)) where = "FROM_START";

            string list = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_HIGH, "{}");
            Regex regex = new Regex(@"^\w+$");
            
            // If `list` would be evaluated more than once (which is the case for LAST,
            // FROM_END, and RANDOM) and is non-trivial, make sure to access it only once.
            if ((where == "LAST" || where == "FROM_END" || where == "RANDOM")
                && !regex.IsMatch(list))
            {
                // We need to create a procedure to avoid reevaluating values.
                string at = Lua.Generator.ValueToCode(block, "AT", Lua.ORDER_NONE, "1");
                string functionName;
                if (mode == "GET")
                {
                    functionName = Lua.Generator.ProvideFunction("list_get_" + where.ToLower(),
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + "(t" +
                        // The value for 'FROM_END' and'FROM_START' depends on `at` so
                        // we add it as a parameter.
                        ((where == "FROM_END" || where == "FROM_START") ? ", at)" : ")\n") +
                        "  return t[" + GetListIndexStr("t", where, "at") + "]\n" +
                        "end");
                }
                else
                {
                    //  mode == 'GET_REMOVE'
                    functionName = Lua.Generator.ProvideFunction("list_remove_" + where.ToLower(),
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + "(t" +
                        // The value for 'FROM_END' and'FROM_START' depends on `at` so
                        // we add it as a parameter.
                        ((where == "FROM_END" || where == "FROM_START") ? ", at)" : ")\n") +
                        "  return table.remove(t, " + GetListIndexStr("t", where, "at") + ")\n" +
                        "end");
                }
                string code = string.Format("{0}({1}{2})", functionName, list,
                    where == "FROM_END" || where == "FROM_START" ? ", " + at : "");
                return new CodeStruct(code, Lua.ORDER_HIGH);
            }
            else
            {
                // Either `list` is a simple variable, or we only need to refer to `list` once.
                int atOrder = (mode == "GET" && where == "FROM_END") ? Lua.ORDER_ADDITIVE : Lua.ORDER_NONE;
                string at = Lua.Generator.ValueToCode(block, "AT", atOrder, "1");
                at = GetListIndexStr(list, where, at);
                if (mode == "GET")
                {
                    string code = list + "[" + at + "]";
                    return new CodeStruct(code, Lua.ORDER_HIGH);
                }
                else
                {
                    string code = "table.remove(" + list + ", " + at + ")";
                    return new CodeStruct(code, Lua.ORDER_HIGH);
                }
            }
        }

        [CodeGenerator(BlockType = "lists_removeIndex")]
        private string List_RemoveIndex(Block block)
        {
            string where = block.GetFieldValue("WHERE");
            if (string.IsNullOrEmpty(where)) where = "FROM_START";

            string list = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_HIGH, "{}");
            Regex regex = new Regex(@"^\w+$");

            // If `list` would be evaluated more than once (which is the case for LAST,
            // FROM_END, and RANDOM) and is non-trivial, make sure to access it only once.
            if ((where == "LAST" || where == "FROM_END" || where == "RANDOM")
                && !regex.IsMatch(list))
            {
                // `list` is an expression, so we may not evaluate it more than once.
                // We can use multiple statements.
                int atOrder = (where == "FROM_END") ? Lua.ORDER_ADDITIVE : Lua.ORDER_NONE;
                string at = Lua.Generator.ValueToCode(block, "AT", atOrder, "1");
                string listVar = Lua.VariableNames.GetDistinctName("tmp_list");
                at = GetListIndexStr(listVar, where, at);

                string code = string.Format("{0} = {1}\ntable.remove({0}, {2})\n", listVar, list, at);
                return code;
            }
            else
            {
                // Either `list` is a simple variable, or we only need to refer to `list` once.
                string at = Lua.Generator.ValueToCode(block, "AT", Lua.ORDER_NONE, "1");
                at = GetListIndexStr(list, where, at);
                string code = "table.remove(" + list + ", " + at + ")";
                return code + "\n";
            }
        }

        [CodeGenerator(BlockType = "lists_setIndex")]
        private CodeStruct List_SetIndex(Block block)
        {
            string mode = block.GetFieldValue("MODE");
            if (string.IsNullOrEmpty(mode)) mode = "SET";

            string where = block.GetFieldValue("WHERE");
            if (string.IsNullOrEmpty(where)) where = "FROM_START";

            string list = Lua.Generator.ValueToCode(block, "LIST", Lua.ORDER_HIGH, "{}");
            string at = Lua.Generator.ValueToCode(block, "AT", Lua.ORDER_ADDITIVE, "1");
            string value = Lua.Generator.ValueToCode(block, "TO", Lua.ORDER_NONE, "None");

            StringBuilder codeSB = new StringBuilder();
            Regex regex = new Regex(@"^\w+$");
            if ((where == "LAST" || where == "FROM_END" || where == "RANDOM")
                && !regex.IsMatch(list))
            {
                string listVar = Lua.VariableNames.GetDistinctName("tmp_list");
                codeSB.Append(listVar + " = " + list + "\n");
                list = listVar;
            }

            if (mode == "SET")
            {
                codeSB.Append(list + "[" + GetListIndexStr(list, where, at) + "] = " + value);
            }
            else
            {
                codeSB.Append(string.Format("table.insert({0}, {1}{2}, {3})", list, GetListIndexStr(list, where, at),
                                            where == "LAST" ? " + 1" : "", value));
            }
            codeSB.Append("\n");
            return new CodeStruct(codeSB.ToString());
        }

        [CodeGenerator(BlockType = "lists_getSublist")]
        private CodeStruct List_GetSubList(Block block)
        {
            string where1 = block.GetFieldValue("WHERE1");
            if (string.IsNullOrEmpty(where1)) where1 = "FROM_START";
            string where2 = block.GetFieldValue("WHERE2");
            if (string.IsNullOrEmpty(where2)) where2 = "FROM_START";

            string list = Lua.Generator.ValueToCode(block, "LIST", Lua.ORDER_HIGH, "{}");
            string at1 = Lua.Generator.ValueToCode(block, "AT1", Lua.ORDER_ADDITIVE, "1");
            string at2 = Lua.Generator.ValueToCode(block, "AT2", Lua.ORDER_ADDITIVE, "1");

            string funcName = Lua.Generator.ProvideFunction("list_sublist_" + where1.ToLower() + "_" + where2.ToLower(),
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + "(source" +
                (where1 == "FROM_END" || where1 == "FROM_START" ? ", at1" : "") +
                (where2 == "FROM_END" || where2 == "FROM_START" ? ", at2" : "") + ")\n" + string.Format(
                @"    local t = {{}}
    local start =  {0}
    local finish =  {1}
    for i = start, finish do
    table.insert(t, source[i])
    end
    return t", GetListIndexStr("source", where1, "at1"), GetListIndexStr("source", where2, "at2")) +
                "\nend");

            string code = funcName + "(" + list +
                          (where1 == "FROM_END" || where1 == "FROM_START" ? ", " + at1 : "") +
                          (where2 == "FROM_END" || where2 == "FROM_START" ? ", " + at2 : "") + ")";
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }

        [CodeGenerator(BlockType = "lists_sort")]
        private CodeStruct List_Sort(Block block)
        {
            string list = Lua.Generator.ValueToCode(block, "LIST", Lua.ORDER_NONE, "{}");
            string direction = block.GetFieldValue("DIRECTION");
            string type = block.GetFieldValue("TYPE");
            string funcName = Lua.Generator.ProvideFunction("list_sort",
                "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(list, typev, direction)
                     local t = {}
                     for n,v in pairs(list) do table.insert(t, v) end
                     local compareFuncs = {
                       NUMERIC = function(a, b)
                         return (tonumber(tostring(a)) or 0)
                             < (tonumber(tostring(b)) or 0) end,
                       TEXT = function(a, b)
                         return tostring(a) < tostring(b) end,
                       IGNORE_CASE = function(a, b)
                         return string.lower(tostring(a)) < string.lower(tostring(b)) end
                     }
                     local compareTemp = compareFuncs[typev]
                     local compare = compareTemp
                     if direction == -1 then 
                        compare = function(a, b) return compareTemp(b, a) end
                     end
                     table.sort(t, compare)
                     return t
                end");
            string code = string.Format(@"{0}({1}, ""{2}"", {3})", funcName, list, type, direction);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "lists_split")]
        private CodeStruct List_Split(Block block)
        {
            string input = Lua.Generator.ValueToCode(block, "INPUT", Lua.ORDER_NONE);
            string delimiter = Lua.Generator.ValueToCode(block, "DELIM", Lua.ORDER_NONE, "\'\'");
            string mode = block.GetFieldValue("MODE");
            if (string.IsNullOrEmpty(mode)) mode = "SPLIT";
            string funcName;
            if (mode.Equals("SPLIT"))
            {
                if (string.IsNullOrEmpty(input))
                    input = "\'\'";
                funcName = Lua.Generator.ProvideFunction("list_string_split",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(input, delim)
                       local t = {}
                       local pos = 1
                       while true do
                         next_delim = string.find(input, delim, pos)
                         if next_delim == nil then
                           table.insert(t, string.sub(input, pos))
                           break
                         else
                           table.insert(t, string.sub(input, pos, next_delim-1))
                           pos = next_delim + #delim
                         end
                       end
                       return t
                    end");
            }
            else if (mode.Equals("JOIN"))
            {
                if (string.IsNullOrEmpty(input)) input = "{}";
                funcName = "table.concat";
            }
            else
            {
                throw new Exception("lists_split Unknown mode: " + mode);
            }
            string code = string.Format("{0}({1}, {2})", funcName, input, delimiter);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
    }
}
