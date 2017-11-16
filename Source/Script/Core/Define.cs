using System;
using System.Collections.Generic;

namespace UBlockly
{
    /// <summary>
    /// Constantly definition in Blockly
    /// </summary>
    public class Define
    {
        public enum EConnection
        {
            None = 0,

            /// <summary>
            /// a right-facing value input
            /// </summary>
            InputValue = 1,

            /// <summary>
            /// a left-facing value output
            /// </summary>
            OutputValue = 2,

            /// <summary>
            /// a down-facing block stack
            /// </summary>
            NextStatement = 3,

            /// <summary>
            /// an up-facing block stack
            /// </summary>
            PrevStatement = 4,

            /// <summary>
            /// an dummy input. Used to add field(s) with no input
            /// </summary>
            DummyInput = 5,
        }

        public enum EAlign
        {
            Left = -1,
            Center = 0,
            Right = 1,
        }

        /// <summary>
        /// Lookup for the opposite type of a connection.
        /// </summary>
        public static EConnection OppositeConnection(EConnection connectionType)
        {
            switch (connectionType)
            {
                case EConnection.InputValue: return EConnection.OutputValue;
                case EConnection.OutputValue: return EConnection.InputValue;
                case EConnection.NextStatement: return EConnection.PrevStatement;
                case EConnection.PrevStatement: return EConnection.NextStatement;
            }
            return EConnection.None;
        }

        /// <summary>
        /// String for use in the 'custom' attribute of a category in toolbox xml.
        /// This string indicates that the category shoud be dynamically populated with
        /// variable blocks
        /// @const(string)
        /// </summary>
        public const string VARIABLE_CATEGORY_NAME = "VARIABLE";

        /// <summary>
        /// String for use in the 'custom' attribute of a category in toolbox xml.
        /// This string indicates that the category shoud be dynamically populated with
        /// procedure blocks.
        /// @const {string}
        /// </summary>
        public const string PROCEDURE_CATEGORY_NAME = "PROCEDURE";

        public const string VARIABLE_GET_BLOCK_TYPE = "variables_get";
        public const string VARIABLE_SET_BLOCK_TYPE = "variables_set";
        public const string DEFINE_NO_RETURN_BLOCK_TYPE = "procedures_defnoreturn";
        public const string DEFINE_WITH_RETURN_BLOCK_TYPE = "procedures_defreturn";
        public const string CALL_NO_RETURN_BLOCK_TYPE = "procedures_callnoreturn";
        public const string CALL_WITH_RETURN_BLOCK_TYPE = "procedures_callreturn";

        /*
            category's name for blocks
        */
        public const string BLOCK_CATEGORY_NAME_LIST = "lists";
        public const string BLOCK_CATEGORY_NAME_CONTROL = "controls";
        public const string BLOCK_CATEGORY_NAME_LOGIC = "logic";
        public const string BLOCK_CATEGORY_NAME_MATH = "math";
        public const string BLOCK_CATEGORY_NAME_PROCEDURE = "procedures";
        public const string BLOCK_CATEGORY_NAME_TEXT = "text";
        public const string BLOCK_CATEGORY_NAME_VARIABLE = "variables";
        public const string BLOCK_CATEGORY_NAME_COROUTINE = "coroutine";
        public const string BLOCK_CATEGORY_NAME_COLOUR = "colour";
        public const string BLOCK_CATEGORY_NAME_UNITTEST = "unittest";

        /// <summary>
        /// a list of field types defined in block
        /// </summary>
        public static string[] FIELD_TYPES = new string[]
        {
            "field_label", "field_input", "field_angle", "field_checkbox", "field_colour",
            "field_variable", "field_dropdown", "field_image", "field_number", "field_date"
        };
         
        /// <summary>
        /// a list of input types defined in block
        /// </summary>
        public static string[] INPUT_TYPES = new string[]
        {
            "input_value", "input_statement", "input_dummy"
        };
        
        /// <summary>
        /// The custom defined datatype in blockly world
        /// </summary>
        public enum EDataType
        {
            Undefined = 0,
            Boolean = 1,
            Number = 2,        //int, float...
            String = 3,
            List = 4
        }
        
        public static Dictionary<EDataType, string[]> DataTypeDB = new Dictionary<EDataType, string[]>()
        {
            {EDataType.Boolean, new[] {"bool", "boolean"}},
            {EDataType.Number, new[] {"float", "int", "double"}},
            {EDataType.String, new[] {"string"}},
            {EDataType.List, new[] {"ArrayList", "list"}}
        };

        /// <summary>
        /// Configure if the field variable's dropdown options add manipulation options: rename, delete.
        /// </summary>
        public const bool FIELD_VARIABLE_ADD_MANIPULATION_OPTIONS = false;
    }
}
