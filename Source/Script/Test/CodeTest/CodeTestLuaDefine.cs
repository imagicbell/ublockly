using System.Text;
using System.Text.RegularExpressions;

namespace PTGame.Blockly
{
    public partial class LuaGenerator
    {
      [CodeGenerator(BlockType = "unittest_main")]
      private string UnitTest_Main(Block block)
      {
        string resultVar = Lua.VariableNames.GetName("unittestResults", Blockly.VARIABLE_CATEGORY_NAME);
        string funcName = Lua.Generator.ProvideFunction("unittest_report",
          "function " + Generator.FUNCTION_NAME_PLACEHOLDER + string.Format(@"()
              -- Create test report.
                 local report = {{}}
                 local summary = {{}}
                 local fails = 0
                 for _, v in pairs({0}) do
                   if v[""success""] then
                      table.insert(summary, ""."")
                    else
                      table.insert(summary, ""F"")
                      fails = fails + 1
                      table.insert(report, ""FAIL: "" .. v[""title""])
                      table.insert(report, v[""log""])
                    end
                  end
                  table.insert(report, 1, table.concat(summary))
                  table.insert(report, """")
                  table.insert(report, ""Number of tests run: "" .. #{0})
                  table.insert(report, """")
                  if fails > 0 then
                    table.insert(report, ""FAILED (failures="" .. fails .. "")"")
                  else
                    table.insert(report, ""OK"")
                  end
                  return table.concat(report, ""\n"")
                end", resultVar));

        Lua.Generator.ProvideFunction("assertEquals",
          "function " + Generator.FUNCTION_NAME_PLACEHOLDER + string.Format(@"(actual, expected, message)
                -- Asserts that a value equals another value.
         assert({0} ~= nil, ""Orphaned assert equals: "" ..  message)
          if type(actual) == ""table"" and type(expected) == ""table"" then
            local lists_match = #actual == #expected
            if lists_match then
              for i, v1 in ipairs(actual) do
                local v2 = expected[i]
                if type(v1) == ""number"" and type(v2) == ""number"" then
                  if math.abs(v1 - v2) > 1e-9 then
                    lists_match = false
                  end
                elseif v1 ~= v2 then
                  lists_match = false
                end
              end
            end
            if lists_match then
              table.insert({0}, {{success=true, log=""OK"", title=message}})
              return
            else
              -- produce the non-matching strings for a human-readable error
              expected = ""{{"" .. table.concat(expected, "", "") .. ""}}""
              actual = ""{{"" .. table.concat(actual, "", "") .. ""}}""
            end
          end
          if actual == expected or (type(actual) == ""number"" and 
             type(expected) == ""number"" and math.abs(actual - expected) < 1e-6) then 
            table.insert({0}, {{success=true, log=""OK"", title=message}})
          else
            table.insert({0},  {{success=false, log=string.format(""Expected: %s\\nActual: %s"", tostring(expected), tostring(actual)), title=message}})
          end
        end", resultVar));
        
        StringBuilder code = new StringBuilder();
        // Setup global to hold test results.
        code.Append(resultVar + " = {}\n");
        // Run tests (unindented).
        string statement = Lua.Generator.StatementToCode(block, "DO");
        Regex regex = new Regex(@"^ ");
        regex.Replace(statement, "");
        regex = new Regex(@"\n ");
        regex.Replace(statement, "\n");
        code.Append(statement);

        string reportVar = Lua.VariableNames.GetDistinctName("report");
        code.Append(reportVar + " = " + funcName + "()\n");
        code.Append(resultVar + " = nil\n");
        code.Append("print(" + reportVar + ")\n");
        return code.ToString();
      }

      [CodeGenerator(BlockType = "unittest_assertequals")]
      private string UnitTest_AssertEquals(Block block)
      {
        string message = Lua.Generator.ValueToCode(block, "MESSAGE", Lua.ORDER_NONE, "");
        string actual = Lua.Generator.ValueToCode(block, "ACTUAL", Lua.ORDER_NONE, "nil");
        string expected = Lua.Generator.ValueToCode(block, "EXPECTED", Lua.ORDER_NONE, "nil");
        if (expected == "TRUE") expected = "true";
        else if (expected == "FALSE") expected = "false";
        else if (expected == "NULL") expected = "nil";
        return string.Format("assertEquals({0}, {1}, {2})\n", actual, expected, message);
      }

      [CodeGenerator(BlockType = "unittest_assertvalue")]
      private string UnitTest_AssertValue(Block block)
      {
        string message = Lua.Generator.ValueToCode(block, "MESSAGE", Lua.ORDER_NONE, "");
        string actual = Lua.Generator.ValueToCode(block, "ACTUAL", Lua.ORDER_NONE, "nil");
        string expected = block.GetFieldValue("EXPECTED");
        if (expected.Equals("TRUE"))
          expected = "true";
        else if (expected.Equals("FALSE"))
          expected = "false";
        else if (expected.Equals("NULL"))
          expected = "nil";
        return string.Format("assertEquals({0}, {1}, {2})\n", actual, expected, message);
      }

      [CodeGenerator(BlockType = "unittest_fail")]
      private string UnitTest_Fail(Block block)
      {
        string resultsVar = Lua.VariableNames.GetName("unittestResults", Blockly.VARIABLE_CATEGORY_NAME);
        string message = Lua.Generator.ValueToCode(block, "MESSAGE", Lua.ORDER_NONE, "");
        string funcName = Lua.Generator.ProvideFunction("unittest_fail",
          "function " + Generator.FUNCTION_NAME_PLACEHOLDER + string.Format(@"(message)
              -- Always assert an error.
               assert({0} ~= nil, ""Orphaned assert fail: "" .. message)
                table.insert({0}, {{success=false, log=""Fail."", title=message}})
          end", resultsVar));
        return funcName + "(" + message + ")\n";
      }

      [CodeGenerator(BlockType = "unittest_adjustindex")]
      private CodeStruct UnitTest_AdjustIndex(Block block)
      {
        string index = Lua.Generator.ValueToCode(block, "INDEX", Lua.ORDER_ADDITIVE, "0");
        float indexValue;
        if (float.TryParse(index, out indexValue))
        {
          return new CodeStruct((indexValue + 1).ToString(), Lua.ORDER_ATOMIC);
        }
        return new CodeStruct(index + "+1", Lua.ORDER_ATOMIC);
      }
    }
}