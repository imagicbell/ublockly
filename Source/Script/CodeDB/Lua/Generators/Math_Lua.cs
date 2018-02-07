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
        [CodeGenerator(BlockType = "math_number")]
        private CodeStruct Math_Number(Block block)
        {
            string code = block.GetFieldValue("NUM");
            float num = float.Parse(code);
            int order = num < 0 ? Lua.ORDER_UNARY : Lua.ORDER_ATOMIC;
            return new CodeStruct(code, order);
        }
        
        [CodeGenerator(BlockType = "math_arithmetic")]
        private CodeStruct Math_Arithmetic(Block block)
        {
            Dictionary<string, KeyValuePair<string, int>> operators = new Dictionary<string, KeyValuePair<string, int>>
            {
                {"ADD", new KeyValuePair<string, int>(" + ", Lua.ORDER_ADDITIVE)},
                {"MINUS", new KeyValuePair<string, int>(" - ", Lua.ORDER_ADDITIVE)},
                {"MULTIPLY", new KeyValuePair<string, int>(" * ", Lua.ORDER_MULTIPLICATIVE)},
                {"DIVIDE", new KeyValuePair<string, int>(" / ", Lua.ORDER_MULTIPLICATIVE)},
                {"POWER", new KeyValuePair<string, int>(" ^ ", Lua.ORDER_EXPONENTIATION)},
            };

            var pair = operators[block.GetFieldValue("OP")];
            string op = pair.Key;
            int order = pair.Value;
            string arg0 = Lua.Generator.ValueToCode(block, "A", order, "0");
            string arg1 = Lua.Generator.ValueToCode(block, "B", order, "0");
            string code = string.Format("{0}{1}{2}", arg0, op, arg1);
            return new CodeStruct(code, order);
        }
        
        [CodeGenerator(BlockType = "math_single")]
        private CodeStruct Math_Single(Block block)
        {
            string op = block.GetFieldValue("OP");
            string code;
            string arg;
            if (op == "NEG")
            {
                // Negation is a special case given its different operator precedence.
                arg = Lua.Generator.ValueToCode(block, "NUM", Lua.ORDER_UNARY, "0");
                return new CodeStruct("-" + arg, Lua.ORDER_UNARY);
            }
            if (op == "SIN" || op == "COS" || op == "TAN")
                arg = Lua.Generator.ValueToCode(block, "NUM", Lua.ORDER_MULTIPLICATIVE, "0");
            else
                arg = Lua.Generator.ValueToCode(block, "NUM", Lua.ORDER_NONE, "0");
            switch (op)
            {
                case "ABS":        code = string.Format("math.abs({0})", arg);             break;
                case "ROOT":       code = string.Format("math.sqrt({0})", arg);            break;
                case "LN":         code = string.Format("math.log({0})", arg);             break;
                case "LOG10":      code = string.Format("math.log10({0})", arg);           break;
                case "EXP":        code = string.Format("math.exp({0})", arg);             break;
                case "POW10":      code = string.Format("math.pow(10, {0})", arg);         break;
                case "ROUND":      code = string.Format("math.floor({0}, .5)", arg);       break;
                case "ROUNDUP":    code = string.Format("math.ceil({0})", arg);            break;
                case "ROUNDDOWN":  code = string.Format("math.floor({0})", arg);           break;
                case "SIN":        code = string.Format("math.sin(math.rad({0}))", arg);   break;
                case "COS":        code = string.Format("math.cos(math.rad({0}))", arg);   break;
                case "TAN":        code = string.Format("math.tan(math.rad({0}))", arg);   break;
                case "ASIN":       code = string.Format("math.deg(math.asin({0}))", arg);  break;
                case "ACOS":       code = string.Format("math.deg(math.acos({0}))", arg);  break;
                case "ATAN":       code = string.Format("math.deg(math.atan({0}))", arg);  break;
                default:           throw new Exception("Unknown math operator: " + op);    
            }
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "math_trig")]
        private CodeStruct Math_Trigonometry(Block block)
        {
            return Math_Single(block);
        }

        [CodeGenerator(BlockType = "math_constant")]
        private CodeStruct Math_Constant(Block block)
        {
            string op = block.GetFieldValue("CONSTANT");
            switch (op)
            {
                case "PI":
                    return new CodeStruct("math.pi", Lua.ORDER_HIGH);
                case "E":
                    return new CodeStruct("math.exp(1)", Lua.ORDER_HIGH);
                case "GOLDEN_RATIO":
                    return new CodeStruct("(1 + math.sqrt(5)) / 2", Lua.ORDER_MULTIPLICATIVE);
                case "SQRT2":
                    return new CodeStruct("math.sqrt(2)", Lua.ORDER_HIGH);
                case "SQRT1_2":
                    return new CodeStruct("math.sqrt(1 / 2)", Lua.ORDER_HIGH);
                case "INFINITY":
                    return new CodeStruct("math.huge", Lua.ORDER_HIGH);
            }
            return CodeStruct.Empty;
        }
        
        [CodeGenerator(BlockType = "math_number_property")]
        private CodeStruct Math_NumberProperty(Block block)
        {
            string numberToCheck = Lua.Generator.ValueToCode(block, "NUMBER_TO_CHECK", Lua.ORDER_MULTIPLICATIVE, "0");
            string property = block.GetFieldValue("PROPERTY");
            string code = null;
            if (property == "PRIME")
            {
                string funcName = Lua.Generator.ProvideFunction("math_isPrime",
                    "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(n)
                        if n == 2 or n == 3 then
                            return true
                        end
                        -- False if n is NaN, negative, is 1, or not whole.
                        -- And false if n is divisible by 2 or 3.
                        if not(n > 1) or n % 1 ~= 0 or n % 2 == 0 or n % 3 == 0 then
                            return false
                        end
                        -- Check all the numbers of form 6k +/- 1, up to sqrt(n).
                        for x = 6, math.sqrt(n) + 1.5, 6 do
                             if n % (x - 1) == 0 or n % (x + 1) == 0 then
                                 return false
                             end
                        end
                        return true
                    end");
                code = string.Format("{0}({1})", funcName, numberToCheck);
                return new CodeStruct(code, Lua.ORDER_HIGH);
            }
            switch (property)
            {
                case "EVEN":     code = numberToCheck + " % 2 == 0"; break;
                case "ODD":      code = numberToCheck + " % 2 == 1"; break;
                case "WHOLE":    code = numberToCheck + " % 1 == 0"; break;
                case "POSITIVE": code = numberToCheck + " > 0";      break;
                case "NEGATIVE": code = numberToCheck + " < 0";      break;
                case "DIVISIBLE_BY":
                    string divisor = Lua.Generator.ValueToCode(block, "DIVISOR", Lua.ORDER_MULTIPLICATIVE);
                    // If 'divisor' is some code that evals to 0, Lua will produce a nan.
                    // Let's produce nil if we can determine this at compile-time.
                    if (string.IsNullOrEmpty(divisor) || divisor.Equals("0"))
                        return new CodeStruct("nil", Lua.ORDER_ATOMIC);
                    code = string.Format("{0} % {1} == 0", numberToCheck, divisor);
                    break;
            }
            return new CodeStruct(code, Lua.ORDER_RELATIONAL);
        }
        
        [CodeGenerator(BlockType = "math_round")]
        private CodeStruct Math_Round(Block block)
        {
            return Math_Single(block);
        }
        
        [CodeGenerator(BlockType = "math_on_list")]
        private CodeStruct Math_OnList(Block block)
        {
            string func = block.GetFieldValue("OP");
            string list = Lua.Generator.ValueToCode(block, "LIST", Lua.ORDER_NONE, "{}");
            string funcName;

            Func<string> provideSum = () =>
            {
                return Lua.Generator.ProvideFunction("math_sum",
                            "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t)
                                local result = 0
                                for _, v in ipairs(t) do
                                    result = result + v
                                end
                                return result
                            end");
            };
            switch (func)
            {
                case "SUM":
                    funcName = provideSum();
                    break;
                case "MIN":
                    funcName = Lua.Generator.ProvideFunction("math_min",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t)
                            if #t == 0 then return 0 end
                            local result = math.huge
                            for _, v in ipairs(t) do
                               if v < result then
                                   result = v
                               end
                            end
                            return result
                        end");
                    break;
                case "MAX":
                    funcName = Lua.Generator.ProvideFunction("math_max",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t)
                            if #t == 0 then return 0 end
                            local result = -math.huge
                            for _, v in ipairs(t) do
                               if v > result then
                                   result = v
                               end
                            end
                            return result
                        end");
                    break;    
                case "MEDIAN":
                    funcName = Lua.Generator.ProvideFunction("math_median",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t)
                            if #t == 0 then return 0 end
                            local temp={}
                            for _, v in ipairs(t) do
                                if type(v) == ""number"" then
                                    table.insert(temp, v)
                                end
                            end
                            table.sort(temp)
                            if #temp % 2 == 0 then
                               return (temp[#temp/2] + temp[(#temp/2)+1]) / 2
                            else
                               return temp[math.ceil(#temp/2)]
                            end
                        end");
                    break;        
                case "AVERAGE":
                    funcName = Lua.Generator.ProvideFunction("math_average",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + string.Format(@"(t)
                            if #t == 0 then return 0 end
                            return {0}(t) / #t
                         end", provideSum()));
                    break;
                case "MODE":
                    // As a list of numbers can contain more than one mode,
                    // the returned result is provided as an array.
                    // The Lua version includes non-numbers.
                    funcName = Lua.Generator.ProvideFunction("math_modes",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t)
                              local counts={}
                              for _, v in ipairs(t) do
                                if counts[v] == nil then
                                  counts[v] = 1
                                else
                                  counts[v] = counts[v] + 1
                                end
                              end
                              local biggestCount = 0
                              for _, v  in pairs(counts) do
                                if v > biggestCount then
                                  biggestCount = v
                                end
                              end
                              local temp={}
                              for k, v in pairs(counts) do
                                if v == biggestCount then
                                  table.insert(temp, k)
                                end
                              end
                              return temp
                        end");
                    break;

                case "STD_DEV":
                    funcName = Lua.Generator.ProvideFunction("math_standard_deviation",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + string.Format(@"(t)
                              local m
                              local vm
                              local total = 0
                              local count = 0
                              local result
                              m = #t == 0 and 0 or {0}(t) / #t
                              for _, v in ipairs(t) do
                                  if type(v) == number then
                                  vm = v - m
                                  total = total + (vm * vm)
                                  count = count + 1
                                end
                              end
                              result = math.sqrt(total / (count-1))
                              return result
                            end", provideSum()));
                    break;
                case "RANDOM":
                    funcName = Lua.Generator.ProvideFunction("math_random_list",
                        "function " + Generator.FUNCTION_NAME_PLACEHOLDER + @"(t)
                              if #t == 0 then
                                return nil
                              end
                              return t[math.random(#t)]
                        end");
                    break;    
                default: throw new Exception("Unknown math operator: " + func);    
            }
            string code = string.Format("{0}({1})", funcName, list);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "math_modulo")]
        private CodeStruct Math_Modulo(Block block)
        {
            string arg0 = Lua.Generator.ValueToCode(block, "DIVIDEND", Lua.ORDER_MULTIPLICATIVE, "0");
            string arg1 = Lua.Generator.ValueToCode(block, "DIVISOR", Lua.ORDER_MULTIPLICATIVE, "0");
            string code = string.Format("{0} % {1}", arg0, arg1);
            return new CodeStruct(code, Lua.ORDER_MULTIPLICATIVE);
        }
        
        [CodeGenerator(BlockType = "math_constrain")]
        private CodeStruct Math_Constrain(Block block)
        {
            // Constrain a number between two limits.
            string arg0 = Lua.Generator.ValueToCode(block, "VALUE", Lua.ORDER_NONE, "0");
            string arg1 = Lua.Generator.ValueToCode(block, "LOW", Lua.ORDER_NONE, "0");
            string arg2 = Lua.Generator.ValueToCode(block, "HIGH", Lua.ORDER_NONE, "0");
            string code = string.Format("math.min(math.max({0}, {1}), {2})", arg0, arg1, arg2);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "math_random_int")]
        private CodeStruct Math_RandomInt(Block block)
        {
            string arg0 = Lua.Generator.ValueToCode(block, "FROM", Lua.ORDER_NONE, "0");
            string arg1 = Lua.Generator.ValueToCode(block, "TO", Lua.ORDER_NONE, "0");
            string code = string.Format("math.random({0}, {1})", arg0, arg1);
            return new CodeStruct(code, Lua.ORDER_HIGH);
        }
        
        [CodeGenerator(BlockType = "math_random_float")]
        private CodeStruct Math_RandomFloat(Block block)
        {
            // Random fraction between 0 and 1.
            return new CodeStruct("math.random()", Lua.ORDER_HIGH);
        }
    }
}
