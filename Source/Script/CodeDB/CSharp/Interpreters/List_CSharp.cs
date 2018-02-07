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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UBlockly
{
    [CodeInterpreter(BlockType = "lists_create_empty")]
    public class Lists_Create_Empty_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            return new DataStruct(new ArrayList());
        }
    }

    [CodeInterpreter(BlockType = "lists_create_with")]
    public class Lists_Create_With_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            ItemListMutator mutator = block.Mutator as ItemListMutator;
            if (mutator == null)
                throw new Exception("Block \"lists_create_with\" must have a mutator \"lists_create_with_item_mutator\"");
                
            ArrayList resultList=new ArrayList();
            for (int i = 0; i < mutator.ItemCount; i++)
            {
                resultList.Add(CSharp.Interpreter.ValueReturn(block, "ADD" + i, new DataStruct(0)));
            }
            return new DataStruct(resultList);
        }
    }

    [CodeInterpreter(BlockType = "lists_repeat")]
    public class Lists_Repeat_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "ITEM", new DataStruct());
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block,"NUM",new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            ArrayList list=new ArrayList();
            int repeatCount = (int) arg1.NumberValue.Value;
            for (int i = 0; i < repeatCount; i++)
            {
                list.Add(arg0.NumberValue);
            }
            ReturnData(new DataStruct(list));
        }
    }

    [CodeInterpreter(BlockType = "lists_reverse")]
    public class Lists_Reverse_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block,"LIST",new DataStruct(new ArrayList()));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            arg0.ListValue.Reverse();
            ReturnData(new DataStruct(arg0.ListValue));
        }
    }

    [CodeInterpreter(BlockType = "lists_isEmpty")]
    public class Lists_IsEmpty_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block,"VALUE",new DataStruct(new ArrayList()));
            yield return ctor;
            DataStruct arg0 = ctor.Data;

            ReturnData(arg0.IsList ? new DataStruct(arg0.ListValue.Count <= 0) : new DataStruct(arg0.StringValue.Length <= 0));
        }
    }

    [CodeInterpreter(BlockType = "lists_length")]
    public class Lists_Length_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(new ArrayList()));
            yield return ctor;
            DataStruct arg0 = ctor.Data;

            ReturnData(arg0.IsList ? new DataStruct(arg0.ListValue.Count) : new DataStruct(arg0.StringValue.Length));
        }
    }

    [CodeInterpreter(BlockType = "lists_indexOf")]
    public class Lists_IndexOf_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "FIND", new DataStruct(""));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            string end = block.GetFieldValue("END");
            switch (end)
            {
                case "FIRST":
                    ReturnData(arg0.IsString
                        ? new DataStruct(arg0.StringValue.IndexOf(arg1.StringValue) + 1)
                        : new DataStruct(arg0.ListValue.IndexOf(arg1.Value) + 1));
                    break;
                case "LAST":
                    ReturnData(arg0.IsString
                        ? new DataStruct(arg0.StringValue.LastIndexOf(arg1.StringValue) + 1)
                        : new DataStruct(arg0.ListValue.LastIndexOf(arg1.Value) + 1));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "lists_getIndex")]
    public class Lists_GetIndex_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(""));
            yield return ctor;
            DataStruct argList = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "AT", new DataStruct(1));
            yield return ctor;
            DataStruct argAt = ctor.Data;
            
            string mode = block.GetFieldValue("MODE");
            string where = block.GetFieldValue("WHERE");
            ArrayList array = argList.ListValue;
            int index = (int)argAt.NumberValue.Value;
            int length = array.Count;
            if (mode.Equals("GET"))
            {
                switch (where)
                {
                    case "FROM_START":
                        ReturnData((DataStruct)array[index]);
                        break;
                    case "FROM_END":
                        ReturnData((DataStruct)array[length-index]);
                        break;
                    case "FIRST":
                        ReturnData((DataStruct)array[0]);
                        break;
                    case "LAST":
                        ReturnData((DataStruct)array[length-1]);
                        break;
                    case "RANDOM":
                        ReturnData((DataStruct)array[new Random().Next(0,length)]);
                        break;
                }
            }
            else
            {
                //GET_REMOVE  
                DataStruct res=new DataStruct();
                int tmp = 0;
                switch (where)
                {
                    case "FROM_START":
                        tmp = index;
                        break;
                    case "FROM_END":
                        tmp = length-index;
                        break;
                    case "FIRST":
                        tmp = 0;
                        break;
                    case "LAST":
                        tmp = length-1;
                        break;
                    case "RANDOM":
                        tmp = new Random().Next(0,length);
                        break;
                }
                res =  (DataStruct)array[tmp];
                array.RemoveAt(length-1);
                ReturnData(res);
            }
        }
    }

    [CodeInterpreter(BlockType = "lists_removeIndex")]
    public class Lists_RemoveIndex_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(""));
            yield return ctor;
            DataStruct argList = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "AT", new DataStruct(1));
            yield return ctor;
            DataStruct argAt = ctor.Data;
            
            string where = block.GetFieldValue("WHERE");
            ArrayList array = argList.ListValue;
            int index = (int)argAt.NumberValue.Value;
            int length = array.Count;
            int tmp = 0;
            switch (where)
            {
                case "FROM_START":
                    tmp = index;
                    break;
                case "FROM_END":
                    tmp = length-index;
                    break;
                case "FIRST":
                    tmp = 0;
                    break;
                case "LAST":
                    tmp = length-1;
                    break;
                case "RANDOM":
                    tmp = new Random().Next(0,length);
                    break;
            }
            argList.ListValue.RemoveAt(tmp);
            
            CSharp.VariableDatas.SetData("VALUE", argList);
        }
    }

    [CodeInterpreter(BlockType = "lists_setIndex")]
    public class Lists_SetIndex_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "LIST", new DataStruct(""));
            yield return ctor;
            DataStruct argList = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "AT", new DataStruct(1));
            yield return ctor;
            DataStruct argAt = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "TO", new DataStruct(1));
            yield return ctor;
            DataStruct changeValue = ctor.Data;
            
            string mode = block.GetFieldValue("MODE");
            string where = block.GetFieldValue("WHERE");
            ArrayList array = argList.ListValue;
            int index = (int)argAt.NumberValue.Value;
            int length = array.Count;
            if(index>=length)
                yield break;
            
            int tmp = 0;
            switch (where)
            {
                case "FROM_START":
                    tmp = index;
                    break;
                case "FROM_END":
                    tmp = length-index;
                    break;
                case "FIRST":
                    tmp = 0;
                    break;
                case "LAST":
                    tmp = length-1;
                    break;
                case "RANDOM":
                    tmp = new Random().Next(0,length);
                    break;
            }
            if(tmp>=length)
                yield break;
            
            if (mode.Equals("SET"))
            {
                argList.ListValue[tmp] = changeValue;
            }
            else
            {
                //INSERT
                argList.ListValue.Insert(tmp,changeValue);
            }
            
            CSharp.VariableDatas.SetData("LIST", argList);
        }
    }

    [CodeInterpreter(BlockType = "lists_getSublist")]
    public class Lists_GetSublist_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "LIST", new DataStruct(""));
            yield return ctor;
            DataStruct argList = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "AT1", new DataStruct(1));
            yield return ctor;
            DataStruct argAt1 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "AT2", new DataStruct(1));
            yield return ctor;
            DataStruct argAt2 = ctor.Data;
            
            string where1 = block.GetFieldValue("WHERE1");
            string where2 = block.GetFieldValue("WHERE2");
            int index1=(int)argAt1.NumberValue.Value;
            int index2=(int)argAt2.NumberValue.Value;
            int lengh = argList.ListValue.Count;
            
            ArrayList result=new ArrayList();

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
                {
                    ReturnData(new DataStruct(result));
                    yield break;
                }

                if (index1 < index2)
                    for (int i = index1; i < (index2 - index1) + 1; i++)
                    {
                        result.Add(i);
                    }
            }

            ReturnData(new DataStruct(result));
        }
    }

    [CodeInterpreter(BlockType = "lists_sort")]
    public class Lists_Sort_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "LIST", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string type = block.GetFieldValue("TYPE");
            string direction = block.GetFieldValue("DIRECTION");
            switch (type)
            {
                case "NUMERIC":
                    arg0.ListValue.ArraySort(CompareNumeric, direction == "-1");
                    ReturnData(new DataStruct(arg0.ListValue));
                    break;
                case "TEXT":
                    arg0.ListValue.ArraySort(CompareText, direction == "-1");
                    ReturnData(new DataStruct(arg0.ListValue));
                    break;
                case "IGNORE_CASE":
                    arg0.ListValue.ArraySort(CompareImgoreCase, direction == "-1");
                    ReturnData(new DataStruct(arg0.ListValue));
                    break;
            }
        }
        
        private bool CompareNumeric(object a, object b)
        {
            return new Number(a.ToString()) > new Number(b.ToString());
        }
        
        private bool CompareText(object a, object b)
        {
            return string.Compare(a.ToString(), b.ToString()) > 0;
        }
        
        private bool CompareImgoreCase(object a, object b)
        {
            return string.Compare(a.ToString().ToLower(), b.ToString().ToLower()) > 0;
        }
    }

    [CodeInterpreter(BlockType = "lists_split_from_text")]
    public class Lists_SplitFromText_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "INPUT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "DELIM", new DataStruct(""));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
             
            ReturnData(new DataStruct(new ArrayList(arg0.StringValue.Split(new string[]{arg1.StringValue},StringSplitOptions.None))));
        }
    }

    [CodeInterpreter(BlockType = "lists_join_text")]
    public class Lists_JoinText_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "INPUT", new DataStruct(""));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "DELIM", new DataStruct(""));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            ReturnData(new DataStruct(string.Join(arg1.StringValue,(string[])arg0.ListValue.ToArray())));
        }
    }

    #region ToolClass
    
    public static class ArrayListExtension
    {
        public static ArrayList ArraySort(this ArrayList list,Func<object,object,bool> compareFunc,bool reverse)
        {
            if (compareFunc != null)
            {
                list.Sort(new ArrayListCompare(compareFunc,reverse));
            }

            return list;
        }
        
        public static bool HasString(this ArrayList list)
        {
            bool hasString = false;
            for (int i = 0; i < list.Count; i++)
            {
                DataStruct ds = (DataStruct) list[i];
                if (ds.IsString)
                {
                    hasString = true;
                }
            }
            return hasString;
        }
        
        public static bool HasList(this ArrayList list)
        {
            bool hasList = false;
            for (int i = 0; i < list.Count; i++)
            {
                DataStruct ds = (DataStruct) list[i];
                if (ds.IsList)
                {
                    hasList = true;
                }
            }
            return hasList;
        }

        public static ArrayList ConvertString(this ArrayList list)
        {
            bool hasString = false;
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                DataStruct ds = (DataStruct) list[i];
                if (!hasString)
                {
                    if (ds.IsString)
                    {
                        hasString = true;
                        index = i;
                    }
                }
                else
                {
                    if (!ds.IsString)
                        list[i] = new DataStruct(ds.ToString());
                }
               
            }
            if (!hasString)
            {
                return list;
            }
            else
            {
                for (int i = 0; i < index; i++)
                {
                    list[i] = new DataStruct(((DataStruct) list[i]).ToString());
                }
                return list;
            }
        }
        
        public static ArrayList ConvertBoolean(this ArrayList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                DataStruct ds = (DataStruct) list[i];
                if (ds.IsBoolean)
                {
                    list[i] = new DataStruct(ds.BooleanValue ? new Number(1): new Number(0));
                }
            }
            return list;
        }
        
        public static DataStruct Sum(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                StringBuilder sumStr = new StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    sumStr.Append(((DataStruct) list[i]).StringValue);
                }
                return new DataStruct(sumStr.ToString());
            }
            list = list.ConvertBoolean();
            Number sum = new Number(0);
            for (int i = 0; i < list.Count; i++)
            {
                sum += ((DataStruct) list[i]).NumberValue;
            }
           return new DataStruct(sum);
        }
        
        public static DataStruct Min(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                string minStr=String.Empty;
                for (int i = 0; i < list.Count; i++)
                {
                    string tmp = ((DataStruct) list[i]).StringValue;
                    if (string.Compare(minStr,tmp)>0)
                        minStr = tmp;
                }
                return new DataStruct(minStr);
            }
            list = list.ConvertBoolean();
            Number min = new Number(0);
            if (list.Count > 0)
            {
                min.Value = float.PositiveInfinity;
                for (int i = 0; i < list.Count; i++)
                {
                    Number tmp=((DataStruct) list[i]).NumberValue;
                    if (min > tmp)
                        min = tmp;
                } 
            }
            return new DataStruct(min);
        }
        
        public static DataStruct Max(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                string maxStr=String.Empty;
                for (int i = 0; i < list.Count; i++)
                {
                    string tmp = ((DataStruct) list[i]).StringValue;
                    if (string.Compare(maxStr,tmp)<0)
                        maxStr = tmp;
                }
                return new DataStruct(maxStr);
            }
            list = list.ConvertBoolean();
            Number max = new Number(0);
            if (list.Count > 0)
            {
                max.Value = float.NegativeInfinity;
                for (int i = 0; i < list.Count; i++)
                {
                    Number tmp=((DataStruct) list[i]).NumberValue;
                    if (max < tmp)
                        max = tmp;
                } 
            }
            return new DataStruct(max);
        }
        
        public static DataStruct Average(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                Number strAverage = new Number(list.Sum().StringValue.ToCharArray().SumCharArray()/list.Count);
                return new DataStruct(strAverage);
            }
            list = list.ConvertBoolean();
            Number average = new Number(0);
            if (list.Count > 0)
            {
                average = list.Sum().NumberValue / new Number(list.Count); 
            }
            return new DataStruct(average);
        }
        
        public static DataStruct Median(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                Number strMedian = new Number(0);
                char[] tmpCharArray = list.Sum().StringValue.ToCharArray();
                Array.Sort(tmpCharArray);
                int length = tmpCharArray.Length;
                if (length > 0)
                {
                    if (length % 2 == 0)
                        strMedian = new Number(((int)tmpCharArray[length / 2] + (int)tmpCharArray[length / 2 + 1]) / 2);
                    else
                        strMedian = new Number((int)tmpCharArray[length / 2 + 1]);
                }
                return new DataStruct(strMedian);
            }
            list = list.ConvertBoolean();
            Number median = new Number(0);
            int count = list.Count;
            if (count > 0)
            {
                list.Sort();
                if (count % 2 == 0)
                    median = (((DataStruct) list[count / 2]).NumberValue + ((DataStruct) list[count / 2+1]).NumberValue) / new Number(2);
                else
                    median = ((DataStruct) list[count / 2+1]).NumberValue;
            }
            return new DataStruct(median);
        }
        
        public static DataStruct Mode(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                ArrayList resultStrList=new ArrayList();
                Dictionary<string, int> strDict = new Dictionary<string, int>();
                int length = list.Count;
                for (int i = 0; i < length; i++)
                {
                    string tmp = ((DataStruct) list[i]).StringValue;
                    if (strDict.ContainsKey(tmp))
                    {
                        strDict[tmp]++;
                    }
                    else
                    {
                        strDict.Add(tmp, 1);
                    }
                }
                int biggerCount = 0;
                foreach (int dictValue in strDict.Values)
                {
                    if (biggerCount < dictValue)
                        biggerCount = dictValue;
                }
                foreach (string s in strDict.Keys)
                {
                    if (biggerCount == strDict[s])
                        resultStrList.Add(new DataStruct(s));
                }
                return new DataStruct(resultStrList);
            }
            list = list.ConvertBoolean();
            int count = list.Count;
            ArrayList resultNumList=new ArrayList();
            if (count > 0)
            {
                Dictionary<Number, int> numDict = new Dictionary<Number, int>();
                for (int i = 0; i < count; i++)
                {
                    Number tmp = ((DataStruct) list[i]).NumberValue;
                    if (numDict.ContainsKey(tmp))
                    {
                        numDict[tmp]++;
                    }
                    else
                    {
                        numDict.Add(tmp, 1);
                    }
                }
                int biggerCount = 0;
                foreach (int dictValue in numDict.Values)
                {
                    if (biggerCount < dictValue)
                        biggerCount = dictValue;
                }
                foreach (Number n in numDict.Keys)
                {
                    if (biggerCount == numDict[n])
                        resultNumList.Add(new DataStruct(n));
                }
            }
            return new DataStruct(resultNumList);
        }
        
        public static DataStruct StdDev(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                return new DataStruct(new Number(0));
            }
            list = list.ConvertBoolean();
            Number resultNum = new Number(0);
            int count = list.Count;
            if (count > 0)
            {
                float totalSquare = 0;
                float average = list.Average().NumberValue.Value;
                for (int i = 0; i < count; i++)
                {
                    Number tmpNum = ((DataStruct) list[i]).NumberValue;
                    float differ = tmpNum.Value - average;
                    totalSquare += differ * differ;
                }
                resultNum=new Number(Math.Sqrt(totalSquare/count));
            }
            return new DataStruct(resultNum);
        }
        
        public static DataStruct Random(this ArrayList list)
        {
            if (list.HasList())
            {
                UnityEngine.Debug.LogError("There is not permit a list!");
                return new DataStruct(-1);
            }
            if (list.HasString())
            {
                list = list.ConvertString();
                int length = list.Count;
                int index = new Random().Next(0, length-1);
                string resultStr = String.Empty;
                resultStr = ((DataStruct) list[index]).StringValue;
                return new DataStruct(resultStr);
            }
            list = list.ConvertBoolean();
            Number resultNum = new Number(0);
            int count = list.Count;
            if (count > 0)
            {
                int index = new Random().Next(0, count-1);
                resultNum = ((DataStruct) list[index]).NumberValue;
            }
            return new DataStruct(resultNum);
        }
    }

    public static class CharArraytExtension
    {
        public static int SumCharArray(this char[] charArray)
        {
            int sum = 0;
            for (int i = 0; i < charArray.Length; i++)
            {
                sum += (int)charArray[i];
            }
            return sum;
        }
    }

    public class ArrayListCompare : IComparer
    {
        private Func<object, object, bool> m_Func;
        private bool m_Reverse;
        
        public ArrayListCompare(Func<object,object,bool> f,bool reverse=false)
        {
            m_Func = f;
            reverse = reverse;
        }   
        public int Compare(object x, object y)
        {
            if (m_Func == null)
                return 0;

            if (m_Reverse)
            {
                if (m_Func(y, x))
                    return 1;
                else
                    return 0;
            }
            else
            {
                if (m_Func(x, y))
                    return 1;
                else
                    return 0;
            }
            
        }
    }
    

    #endregion
    
}
