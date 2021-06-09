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
using System.Text;

namespace UBlockly
{
    [CodeInterpreter(BlockType = "math_number")]
    public class MathNumberCmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            string value = block.GetFieldValue("NUM");
            Number num = new Number(value);
            return new DataStruct(num);
        }
    }

    [CodeInterpreter(BlockType = "math_arithmetic")]
    public class Math_Arithmetic_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "A", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "B", new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            string op = block.GetFieldValue("OP");
            switch (op)
            {
                case "ADD":
                    ReturnData(new DataStruct(arg0.NumberValue + arg1.NumberValue));
                    break;
                case "MINUS":
                    ReturnData(new DataStruct(arg0.NumberValue - arg1.NumberValue));
                    break;
                case "MULTIPLY":
                    ReturnData(new DataStruct(arg0.NumberValue * arg1.NumberValue));
                    break;
                case "DIVIDE":
                    ReturnData(new DataStruct(arg0.NumberValue / arg1.NumberValue));
                    break;
                case "POWER":
                    ReturnData(new DataStruct(System.Math.Pow(arg0.NumberValue.Value, arg1.NumberValue.Value)));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "math_single")]
    public class Math_Single_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "NUM", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string op = block.GetFieldValue("OP");
            switch (op)
            {
                case "ROOT":
                    ReturnData(new DataStruct(System.Math.Sqrt(arg0.NumberValue.Value)));
                    break;
                case "ABS":
                    ReturnData(new DataStruct(System.Math.Abs(arg0.NumberValue.Value)));
                    break;
                case "NEG":
                    ReturnData(new DataStruct(-arg0.NumberValue.Value));
                    break;
                case "LN":
                    ReturnData(new DataStruct(System.Math.Log(arg0.NumberValue.Value)));
                    break;
                case "LOG10":
                    ReturnData(new DataStruct(System.Math.Log10(arg0.NumberValue.Value)));
                    break;
                case "EXP":
                    ReturnData(new DataStruct(System.Math.Exp(arg0.NumberValue.Value)));
                    break;
                case "POW10":
                    ReturnData(new DataStruct(System.Math.Pow(10,arg0.NumberValue.Value)));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "math_trig")]
    public class Math_Trig_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "NUM", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string op = block.GetFieldValue("OP");
            switch (op)
            {
                case "SIN":
                    ReturnData(new DataStruct(System.Math.Sin(arg0.NumberValue.Value)));
                    break;
                case "COS":
                    ReturnData(new DataStruct(System.Math.Cos(arg0.NumberValue.Value)));
                    break;
                case "TAN":
                    ReturnData(new DataStruct(System.Math.Tan(arg0.NumberValue.Value)));
                    break;
                case "ASIN":
                    ReturnData(new DataStruct(System.Math.Asin(arg0.NumberValue.Value)));
                    break;
                case "ACOS":
                    ReturnData(new DataStruct(System.Math.Acos(arg0.NumberValue.Value)));
                    break;
                case "ATAN":
                    ReturnData(new DataStruct(System.Math.Atan(arg0.NumberValue.Value)));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "math_constant")]
    public class Math_Constant_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            string op = block.GetFieldValue("CONSTANT");
            switch (op)
            {
                case "PI":
                    return new DataStruct(System.Math.PI);
                case "E":
                    return new DataStruct(System.Math.E);
                case "GOLDEN_RATIO":
                    return new DataStruct(1+System.Math.Sqrt(5)/2.0f);
                case "SQRT2":
                    return new DataStruct(System.Math.Sqrt(2.0f));
                case "SQRT1_2":
                    return new DataStruct(System.Math.Sqrt(1/2.0f));
                case "INFINITY":
                    return new DataStruct(float.PositiveInfinity);
            }
            return DataStruct.Undefined;
        }
    }

    [CodeInterpreter(BlockType = "math_number_property")]
    public class Math_NumberProperty_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "NUMBER_TO_CHECK", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string op = block.GetFieldValue("PROPERTY");
            switch (op)
            {
                case "EVEN":
                    ReturnData(new DataStruct(arg0.NumberValue.Value % 2 == 0));
                    break;
                case "ODD":
                    ReturnData(new DataStruct(arg0.NumberValue.Value % 2 == 1));
                    break;
                case "PRIME":
                    ReturnData(new DataStruct(CheckPrime(arg0.NumberValue)));
                    break;
                case "WHOLE":
                    ReturnData(new DataStruct(arg0.NumberValue.Value % 1 == 0));
                    break;
                case "POSITIVE":
                    ReturnData(new DataStruct(arg0.NumberValue.Value > 0));
                    break;
                case "NEGATIVE":
                    ReturnData(new DataStruct(arg0.NumberValue.Value < 0));
                    break;
                case "DIVISIBLE_BY":
                    ctor = CSharp.Interpreter.ValueReturn(block, "DIVISOR", new DataStruct(0));
                    yield return ctor;
                    DataStruct arg1 = ctor.Data;
                    if (arg1.NumberValue.Value == 0)
                        ReturnData(DataStruct.Undefined);
                    else
                        ReturnData(new DataStruct((arg0.NumberValue % arg1.NumberValue).Value == 0));
                    break;
            }
        }
        
        public bool CheckPrime(Number num)
        {
            if (num.Value == 2 || num.Value == 3)
                return true;
            //False if n is NaN, negative, is 1, or not whole.
            //And false if n is divisible by 2 or 3.
            if (!(num.Value > 1) || num.Value % 1 != 0 || num.Value % 2 == 0 || num.Value % 3 == 0)
                return false;
            //Check all the numbers of form 6k +/- 1, up to sqrt(n).
            for (int x = 6; x > System.Math.Sqrt(num.Value)+1.5f; x--)
            {
                if (num.Value % (x - 1) == 0 || num.Value % (x + 1) == 0)
                    return false;
            }
            return true;
        }
    }

    [CodeInterpreter(BlockType = "math_round")]
    public class Math_Round_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "NUM", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string op = block.GetFieldValue("OP");
            switch (op)
            {
                case "ROUND":
                    ReturnData(new DataStruct(System.Math.Round(arg0.NumberValue.Value)));
                    break;
                case "ROUNDUP":
                    ReturnData(new DataStruct(System.Math.Ceiling(arg0.NumberValue.Value)));
                    break;
                case "ROUNDDOWN":
                    ReturnData(new DataStruct(System.Math.Floor(arg0.NumberValue.Value)));
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "math_on_list")]
    public class Math_OnList_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "LIST", new DataStruct(new ArrayList()));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            string op = block.GetFieldValue("OP");
            switch (op)
            {
                case "SUM":
                    ReturnData(arg0.ListValue.Sum());
                    break;
                case "MIN":
                    ReturnData(arg0.ListValue.Min());
                    break;
                case "MAX":
                    ReturnData(arg0.ListValue.Max());
                    break;
                case "AVERAGE":
                    ReturnData(arg0.ListValue.Average());
                    break;
                case "MEDIAN":
                    ReturnData(arg0.ListValue.Median());
                    break;
                case "MODE":
                    ReturnData(arg0.ListValue.Mode());
                    break;
                case "STD_DEV":
                    ReturnData(arg0.ListValue.StdDev());
                    break;
                case "RANDOM":
                    ReturnData(arg0.ListValue.Random());
                    break;
            }
        }
    }

    [CodeInterpreter(BlockType = "math_modulo")]
    public class Math_Modulo_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "DIVIDEND", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "DIVISOR", new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
                
            ReturnData(new DataStruct(arg0.NumberValue.Value % arg1.NumberValue.Value));
        }
    }

    [CodeInterpreter(BlockType = "math_constrain")]
    public class Math_Constrain_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "VALUE", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "LOW", new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "HIGH", new DataStruct(0));
            yield return ctor;
            DataStruct arg2 = ctor.Data;
            
            ReturnData(new DataStruct(System.Math.Min(System.Math.Max(arg0.NumberValue.Value, arg1.NumberValue.Value),arg2.NumberValue.Value)));
        }
    }

    [CodeInterpreter(BlockType = "math_random_int")]
    public class Math_RandomInt_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            CmdEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "FROM", new DataStruct(0));
            yield return ctor;
            DataStruct arg0 = ctor.Data;
            
            ctor = CSharp.Interpreter.ValueReturn(block, "TO", new DataStruct(0));
            yield return ctor;
            DataStruct arg1 = ctor.Data;
            
            ReturnData(new DataStruct(UnityEngine.Random.Range((int)arg0.NumberValue.Value,(int)arg1.NumberValue.Value)));
        }
    }

    [CodeInterpreter(BlockType = "math_random_float")]
    public class Math_RandomFloat_Cmdtor : ValueCmdtor
    {
        protected override DataStruct Execute(Block block)
        {
            return new DataStruct(UnityEngine.Random.value);
        }
    }
}
