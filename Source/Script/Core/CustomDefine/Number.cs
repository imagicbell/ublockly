using System;

namespace UBlockly
{
    public enum NumberType
    {
        NaN,
        Int,
        Float,
        Double,
    }

    /// <summary>
    /// struct for number type, instead of int, float, double...
    /// we use number in blockly, like number type in dynamic languages like javascript.
    /// </summary>
    public struct Number
    {
        public bool IsNaN;
        public float Value;

        public Number(int intValue)
        {
            IsNaN = false;
            Value = intValue;
        }

        public Number(float floatValue)
        {
            IsNaN = false;
            Value = floatValue;
        }

        public Number(double doubleValue)
        {
            //current only support up to float types in blockly, truly which is enough :)
            IsNaN = false;
            Value = (float) doubleValue;
        }

        public Number(string strValue)
        {
            double doubleValue;

            if (float.TryParse(strValue, out Value))
            {
                IsNaN = false;
            }
            else if (double.TryParse(strValue, out doubleValue))
            {
                //try parse using double
                IsNaN = false;
                Value = (float) doubleValue;
            }
            else
            {
                IsNaN = true;
                UnityEngine.Debug.LogWarning("Number constructor must have a string argument with number value.");
            }
        }

        public static Number MinValue
        {
            get { return new Number(float.MinValue); }
        }

        public static Number MaxValue
        {
            get { return new Number(float.MaxValue); }
        }

        public static Number NaN
        {
            get { return new Number {IsNaN = true}; }
        }

        public override string ToString()
        {
            if (IsNaN) return "NaN";
            return Value.ToString();
        }

        public static Number operator +(Number a, Number b)
        {
            return new Number(a.Value + b.Value);
        }

        public static Number operator -(Number a, Number b)
        {
            return new Number(a.Value - b.Value);
        }
        
        public static Number operator -(Number a)
        {
            return new Number(-a.Value);
        }
        
        public static Number operator *(Number a, Number b)
        {
            return new Number(a.Value * b.Value);
        }
        
        public static Number operator /(Number a, Number b)
        {
            return new Number(a.Value / b.Value);
        }
        
        public static Number operator %(Number a, Number b)
        {
            return new Number(a.Value % b.Value);
        }
        
        public static bool operator ==(Number a, Number b)
        {
            return Math.Abs(a.Value - b.Value) < 9.99999943962493E-11;
        }
        
        public static bool operator !=(Number a, Number b)
        {
            return Math.Abs(a.Value - b.Value) >= 9.99999943962493E-11;
        }

        public static bool operator <(Number a, Number b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >(Number a, Number b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <=(Number a, Number b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator >=(Number a, Number b)
        {
            return a.Value >= b.Value;
        }

        public void Clamp(Number min, Number max)
        {
            Value = UnityEngine.Mathf.Clamp(Value, min.Value, max.Value);
        }
    }
}
