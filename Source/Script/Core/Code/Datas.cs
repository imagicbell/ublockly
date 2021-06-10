/****************************************************************************

Copyright 2016 sophieml1989@gmail.com
Copyright 2016 dtknowlove@qq.com

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
using System.Text;

namespace UBlockly
{
    /// <summary>
    /// 分别定义boolean, number, string, 避免封箱、拆箱
    /// </summary>
    public struct DataStruct
    {
        public Define.EDataType Type;

        private bool mBooleanValue;
        public bool BooleanValue
        {
            get
            {
                if (this.Type != Define.EDataType.Boolean)
                    throw new Exception("try to GET a boolean value from a not-boolean data");
                return mBooleanValue;
            }
            set
            {
                if (this.Type != Define.EDataType.Boolean)
                    throw new Exception("try to SET a boolean value from a not-boolean data");
                mBooleanValue = value;
            }
        }

        private Number mNumberValue;
        public Number NumberValue
        {
            get
            {
                if (this.Type != Define.EDataType.Number)
                    throw new Exception("try to GET a number value from a not-number data");
                return mNumberValue;
            }
            set
            {
                if (this.Type != Define.EDataType.Number)
                    throw new Exception("try to SET a number value from a not-number data");
                mNumberValue = value;
            }
        }

        private string mStringValue;
        public string StringValue
        {
            get
            {
                if (this.Type != Define.EDataType.String)
                    throw new Exception("try to GET a string value from a not-string data");
                return mStringValue;
            }
            set
            {
                if (this.Type != Define.EDataType.String)
                    throw new Exception("try to SET a string value from a not-string data");
                mStringValue = value;
            }
        }

        private ArrayList mListValue;
        public ArrayList ListValue
        {
            get
            {
                if (this.Type != Define.EDataType.List)
                    throw new Exception("try to GET a list value from a not-list data");
                return mListValue;
            }
            set
            {
                if (this.Type != Define.EDataType.List)
                    throw new Exception("try to SET a list value from a not-list data");
                mListValue = value;
            }
        }

        public DataStruct(bool booleanValue)
        {
            this.Type = Define.EDataType.Boolean;
            this.mBooleanValue = booleanValue;
            this.mNumberValue = Number.NaN;
            this.mStringValue = null;
            this.mListValue = null;
        }

        public DataStruct(Number numberValue)
        {
            this.Type = Define.EDataType.Number;
            this.mBooleanValue = false;
            this.mNumberValue = numberValue;
            this.mStringValue = null;
            this.mListValue = null;
        }

        public DataStruct(int intValue)
        {
            this.Type = Define.EDataType.Number;
            this.mBooleanValue = false;
            this.mNumberValue = new Number(intValue);
            this.mStringValue = null;
            this.mListValue = null;
        }
        
        public DataStruct(float floatValue)
        {
            this.Type = Define.EDataType.Number;
            this.mBooleanValue = false;
            this.mNumberValue = new Number(floatValue);
            this.mStringValue = null;
            this.mListValue = null;
        }
        
        public DataStruct(double doubleValue)
        {
            this.Type = Define.EDataType.Number;
            this.mBooleanValue = false;
            this.mNumberValue = new Number(doubleValue);
            this.mStringValue = null;
            this.mListValue = null;
        }

        public DataStruct(string stringValue)
        {
            this.Type = Define.EDataType.String;
            this.mBooleanValue = false;
            this.mNumberValue = Number.NaN;
            this.mStringValue = stringValue;
            this.mListValue = null;
        }
        
        public DataStruct(ArrayList listValue)
        {
            this.Type = Define.EDataType.List;
            this.mBooleanValue = false;
            this.mNumberValue = Number.NaN;
            this.mStringValue = null;
            this.mListValue = listValue;
        }

        public static DataStruct Undefined
        {
            get { return new DataStruct(); }
        }

        public bool IsUndefined
        {
            get { return Type <= 0; }
        }

        public bool IsBoolean
        {
            get { return Type == Define.EDataType.Boolean; }
        }

        public bool IsNumber
        {
            get { return Type == Define.EDataType.Number; }
        }

        public bool IsString
        {
            get { return Type == Define.EDataType.String; }
        }

        public bool IsList
        {
            get { return Type == Define.EDataType.List; }
        }

        #region override

        public override bool Equals(object obj)
        {
            return (obj is DataStruct) && (this == (DataStruct) obj);
        }

        public override string ToString()
        {
            switch (this.Type)
            {
                case Define.EDataType.Undefined: return "Undefined";
                case Define.EDataType.Boolean: return mBooleanValue.ToString();
                case Define.EDataType.Number: return mNumberValue.ToString();
                case Define.EDataType.String: return mStringValue;
                case Define.EDataType.List:
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var e in mListValue)
                        sb.Append(e);
                    return sb.ToString();
                }
                default: return "Undefined";
            }
        }
        
        public static bool operator ==(DataStruct a, DataStruct b)
        {
            if (a.Type != b.Type)
                return false;
            switch (a.Type)
            {
                case Define.EDataType.Undefined: return true;
                case Define.EDataType.Boolean: return a.BooleanValue == b.BooleanValue;
                case Define.EDataType.Number: return a.NumberValue == b.NumberValue;
                case Define.EDataType.String: return a.StringValue == b.StringValue;
                case Define.EDataType.List:
                {
                    if (a.ListValue.Count != b.ListValue.Count)
                        return false;
                    for (int i = 0; i < a.ListValue.Count; i++)
                    {
                        if (a.ListValue[i] != b.ListValue[i])
                            return false;
                    }
                    return true;
                }
                default: return false;
            }
        }

        public static bool operator !=(DataStruct a, DataStruct b)
        {
            return !(a == b);
        }


        #endregion
       
    }
    
    /// <summary>
    /// hold the actual data for variables
    /// </summary>
    public class Datas
    {
        private Dictionary<string, DataStruct> mDB = null;

        public Datas()
        {
            mDB = new Dictionary<string, DataStruct>();
        }

        public void Reset()
        {
            mDB.Clear();
        }

        /// <summary>
        /// get variable data 
        /// </summary>
        public DataStruct GetData(string varName)
        {
            DataStruct data;
            return mDB.TryGetValue(varName, out data) ? data : DataStruct.Undefined;
        }

        /// <summary>
        /// set variable data
        /// </summary>
        public void SetData(string varName, DataStruct data)
        {
            mDB[varName] = data;
        }
    }
}
