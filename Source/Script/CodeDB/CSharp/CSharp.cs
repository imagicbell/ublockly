/****************************************************************************

c# code generating and interpreting collection

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

namespace UBlockly
{
    public static class CSharp
    {
        private static CSharpGenerator mGenerator = null;
        public static CSharpGenerator Generator
        {
            get { return mGenerator ?? (mGenerator = new CSharpGenerator(VariableNames)); }
        }
        
        private static CSharpInterpreter mInterpreter = null;
        public static CSharpInterpreter Interpreter
        {
            get { return mInterpreter ?? (mInterpreter = new CSharpInterpreter(VariableNames, VariableDatas)); }
        }

        private static Names mVariableNames = null;
        public static Names VariableNames
        {
            get
            {
                return mVariableNames ?? (mVariableNames = new Names(
                           //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/
                           @"abstract, as,	base, bool, break, byte, case, catch,
                              char, checked, class, const, continue, decimal, default, delegate,
                              do, double, else, enum, event, explicit, extern, false, 
                              finally, fixed, float, for, foreach, goto, if, implicit,
                              in, int, interface, internal, is, lock, long, namespace, new, null, object,
                              operator, out, override, params, private, protected, public,
                              readonly, ref, return, sbyte, sealed, short, sizeof, stackalloc,
                              static, string, struct, switch, this, throw, true, try, 
                              typeof, uint, ulong, unchecked, unsafe, ushort, using, virtual, void, volatile, while,
                              add, alias, ascending, async, await, descending, dynamic, from, get,
                              global, group, into, join, let, nameof, orderby, partial, 
                              remove, select, set, value, var, when, where, yield"));
            }
        }

        private static Datas mVariableDatas = null;
        public static Datas VariableDatas
        {
            get { return mVariableDatas ?? (mVariableDatas = new Datas()); }
        }

        public const int ORDER_ATOMIC = 0;          // literals
        
        //https://msdn.microsoft.com/en-us/library/2bxt6kc4.aspx
        public const int ORDER_EXPRESSION = 1;      // [ ] ( ) . –> postfix ++ and postfix
        public const int ORDER_UNARY = 2;           // prefix ++ and prefix –– sizeof & * + – ~ !
        public const int ORDER_TYPECAST = 3;        // (T)x
        public const int ORDER_MULTIPLICATIVE = 4;  // * / %
        public const int ORDER_ADDITIVE = 5;        // + -
        public const int ORDER_BITWISE = 6;         // << >>
        public const int ORDER_RELATIONAL = 7;      // < > <= >=
        public const int ORDER_EQUALITY = 8;        // == !=
        public const int ORDER_BITWISE_AND = 9;     // &
        public const int ORDER_BITWISE_XOR = 10;    // ^
        public const int ORDER_BITWISE_OR = 11;     // |
        public const int ORDER_LOGICAL_AND = 12;    // &&
        public const int ORDER_LOGICAL_OR = 13;     // ||
        public const int ORDER_CONDITIONAL = 14;    // ?:
        public const int ORDER_ASSIGNMENT = 15;     // = *= /= %= += –= <<= >>= &= ^= |=
        public const int ORDER_COMMA = 16;          // ,
        
        public const int ORDER_NONE = 99;
        
                
        public static void Dispose()
        {
            mGenerator = null;
            mInterpreter = null;
            mVariableNames = null;
            mVariableDatas = null;
        }
    }
}
