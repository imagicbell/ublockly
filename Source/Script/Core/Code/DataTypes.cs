using System.Collections;
using System.Collections.Generic;

namespace PTGame.Blockly
{
    /// <summary>
    /// define value types, such as boolean, number(int, float...), string. 
    /// </summary>
    public class DataTypes
    {
        public const int Undefined = 0;
        public const int Boolean = 1;
        public const int Number = 2;        //int, float...
        public const int String = 3;
        public const int List = 4;

        public static Dictionary<int, string[]> DB = new Dictionary<int, string[]>()
        {
            {Boolean, new[] {"bool", "boolean"}},
            {Number, new[] {"float", "int", "double"}},
            {String, new[] {"string"}},
            {List, new[] {"ArrayList", "list"}}
        };
    }
}