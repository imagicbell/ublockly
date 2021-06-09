/****************************************************************************

Functions for interpreting c# code for blocks.

Copyright 2016 dtknowlove@qq.com
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
using System.Collections;
using System.Linq;

namespace UBlockly
{
    [CodeInterpreter(BlockType = "text")]
    public class Text_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            string value = block.GetFieldValue("TEXT");
            return new DataStruct(value);
        }
    }

    [CodeInterpreter(BlockType = "text_print")]
    public class Text_Print_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct input = ctor.Data; 
            //todo: 暂时用Debug.Log，后面根据UI输出框再定
            UnityEngine.Debug.Log("c# print: " + input.ToString());
        }
    }

    [CodeInterpreter(BlockType = "text_join")]
    public class Text_Join_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            ItemListMutator mutator = block.Mutator as ItemListMutator;
            if (mutator == null)
                throw new Exception("Block \"text_join\" must have a mutater \"text_join_mutator\"");
            string result = String.Empty;
            if (mutator.ItemCount > 0)
            {
                string[] elements = new string[mutator.ItemCount];
                for (int i = 0; i < mutator.ItemCount; i++)
                {
                    CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "ADD" + i, new DataStruct(""));
                    yield return ctor;
                    elements[i] = ctor.Data.StringValue;
                }
                result = string.Join("", elements);
            }
            ReturnData(new DataStruct(result));
        }
    }

    [CodeInterpreter(BlockType = "text_append")]
    public class Text_Append_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string tmp = block.GetFieldValue("VAR");
            
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string result = CSharp.VariableDatas.GetData(tmp) + arg0.StringValue;
            CSharp.VariableDatas.SetData(tmp, new DataStruct(result));
        }
    }

    [CodeInterpreter(BlockType = "text_length")]
    public class Text_Length_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;

            DataStruct returnData = arg0.IsString ? new DataStruct(arg0.StringValue.Length) : new DataStruct(arg0.ListValue.Count);
            ReturnData(returnData);
        }
    }

    [CodeInterpreter(BlockType = "text_isEmpty")]
    public class Text_IsEmpty_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            DataStruct returnData = arg0.IsString 
                ? new DataStruct(string.IsNullOrEmpty(arg0.StringValue)) 
                : new DataStruct(arg0.ListValue.Count <= 0);
            ReturnData(returnData);
        }
    }

    [CodeInterpreter(BlockType = "text_indexOf")]
    public class Text_IndexOf_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "FIND", new DataStruct(""));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            string end = block.GetFieldValue("END");
            switch (end)
            {
                case "FIRST":
                    ReturnData(new DataStruct(arg0.StringValue.IndexOf(arg1.StringValue) + 1));
                    break;
                case "LAST":
                    ReturnData(new DataStruct(arg0.StringValue.LastIndexOf(arg1.StringValue) + 1));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "text_charAt")]
    public class Text_CharAt_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "Value", new DataStruct(""));
            yield return ctor;
            DataStruct arg0Value = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "AT", new DataStruct(1));
            yield return ctor;
            DataStruct arg1At = ctor.Data;
            
            string where = block.GetFieldValue("WHERE");
            string result=String.Empty;
            int length = arg0Value.StringValue.Length;
            int index = (int)arg1At.NumberValue.Value;
            if (length > index)
            {
                switch (where)
                {
                    case "FROM_START":
                        result = arg0Value.StringValue[index].ToString();
                        break;
                    case "FIRST":
                        result = arg0Value.StringValue[0].ToString();
                        break;
                    case "FROM_END":
                        result = arg0Value.StringValue[length-index].ToString();
                        break;
                    case "LAST":
                        result = arg0Value.StringValue[length-1].ToString();
                        break;
                    case "RANDOM":
                        result = arg0Value.StringValue[new Random().Next(0,length)].ToString();
                        break;
                }
            }
            ReturnData(new DataStruct(result));
        }
    }

    [CodeInterpreter(BlockType = "text_getSubstring")]
    public class Text_GetSubstring_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "STRING", new DataStruct(""));
            yield return ctor;
            DataStruct arg0Value = ctor.Data;

            ctor = CSharp.Interpreter.ValueReturn(block, "AT1", new DataStruct(1));
            yield return ctor;
            DataStruct argAt1 = ctor.Data;

            ctor = CSharp.Interpreter.ValueReturn(block, "AT2", new DataStruct(1));
            yield return ctor;
            DataStruct argAt2 = ctor.Data;
            
            string where1 = block.GetFieldValue("WHERE1");
            string where2 = block.GetFieldValue("WHERE2");
            string result=String.Empty;
            int lengh = arg0Value.StringValue.Length;
            int index1 = (int)argAt1.NumberValue.Value;
            int index2 = (int)argAt2.NumberValue.Value;
            if (index1 < lengh && index2 < lengh)
            {
                switch (where1)
                {
                    case "FROM_START":
                        break;
                    case "FROM_END":
                        index1 = lengh - index1;
                        break;
                    case "FIRST":
                        index1 = 0;
                        break;  
                }
                switch (where2)
                {
                    case "FROM_START":
                        break;
                    case "FROM_END":
                        index2 = lengh - index2;
                        break;
                    case "LAST":
                        index2 = lengh - 1;
                        break;  
                }
                if (index1 < 0 || index2 < 0)
                    result = String.Empty;
                if (index1 < index2)
                {
                    int subLength = index2 - index1 + 1;
                    result = arg0Value.StringValue.Substring(index1,subLength);
                }
            }
            ReturnData(new DataStruct(result));
        }
    }

    [CodeInterpreter(BlockType = "text_changeCase")]
    public class Text_ChangeCase_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data; 
            string op = block.GetFieldValue("CASE");
            switch (op)
            {
                case "UPPERCASE":
                    ReturnData(new DataStruct(arg0.StringValue.ToUpper()));
                    break;
                case "LOWERCASE":
                    ReturnData(new DataStruct(arg0.StringValue.ToLower()));
                    break;
                case "TITLECASE":
                    string result = "";
                    bool inWord = false;
                    for (int i = 0; i < arg0.StringValue.Length; i++)
                    {
                        if (inWord)
                        {
                            result += arg0.StringValue[i].ToString().ToLower();
                            if (arg0.StringValue[i].ToString() == " ")
                            {
                                inWord = false;
                            }
                        }
                        else
                        {
                            result += arg0.StringValue[i].ToString().ToUpper();
                            if (arg0.StringValue[i].ToString() != " ")
                                inWord = true;
                        }
                    }
                    ReturnData(new DataStruct(result));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "text_trim")]
    public class Text_Trim_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string mode = block.GetFieldValue("MODE");
            switch (mode)
            {
                case "BOTH":
                    ReturnData(new DataStruct(arg0.StringValue.Trim()));
                    break;
                case "LEFT":
                    ReturnData(new DataStruct(arg0.StringValue.TrimStart()));
                    break;
                case "RIGHT":
                    ReturnData(new DataStruct(arg0.StringValue.TrimEnd()));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "text_count")]
    public class Text_Count_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "SUB", new DataStruct(""));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            int count = 0;
            string tmp = arg0.StringValue;
            while (true)
            {
                if (tmp.Contains(arg1.StringValue))
                {
                    count++;
                    tmp = tmp.Substring(tmp.IndexOf(arg1.StringValue) + 1);
                }
                else
                {
                    break;
                }
            }
            ReturnData(new DataStruct(count));
        }
    }

    [CodeInterpreter(BlockType = "text_replace")]
    public class Text_Replace_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;

            ctor = CSharp.Interpreter.ValueReturn(block, "FROM", new DataStruct(""));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "TO", new DataStruct(""));
            yield return ctor;
            DataStruct arg2 = ctor.Data;

            ReturnData(new DataStruct(arg0.StringValue.Replace(arg1.StringValue, arg2.StringValue)));
        }
    }

    [CodeInterpreter(BlockType = "text_reverse")]
    public class Text_Reverse_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TEXT", new DataStruct(""));
            yield return ctor;
            DataStruct returnData = new DataStruct(new string(ctor.Data.StringValue.Reverse().ToArray()));
            ReturnData(returnData);
        }
    }
}
