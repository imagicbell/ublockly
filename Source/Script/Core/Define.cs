/****************************************************************************

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
        /// String for use in the 'custom' attribute of a category in toolbox.
        /// This string indicates that the category shoud be dynamically populated with variable blocks
        /// </summary>
        public const string VARIABLE_CATEGORY_NAME = "VARIABLE";

        /// <summary>
        /// String for use in the 'custom' attribute of a category in toolbox.
        /// This string indicates that the category shoud be dynamically populated with procedure blocks.
        /// </summary>
        public const string PROCEDURE_CATEGORY_NAME = "PROCEDURE";

        public const string VARIABLE_GET_BLOCK_TYPE = "variables_get";
        public const string VARIABLE_SET_BLOCK_TYPE = "variables_set";
        public const string DEFINE_NO_RETURN_BLOCK_TYPE = "procedures_defnoreturn";
        public const string DEFINE_WITH_RETURN_BLOCK_TYPE = "procedures_defreturn";
        public const string CALL_NO_RETURN_BLOCK_TYPE = "procedures_callnoreturn";
        public const string CALL_WITH_RETURN_BLOCK_TYPE = "procedures_callreturn";

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
        
        /// <summary>
        /// CLOCKWISE and OFFSET work together to set the behaviour of the angle picker.  
        /// While many combinations are possible, two modes are typical: 
        /// Math mode. 0 deg is right, 90 is up.  This is the style used by protractors.
        ///    CLOCKWISE = false;
        ///    OFFSET = 0; 
        /// Compass mode. 0 deg is up, 90 is right.  This is the style used by maps.
        ///    CLOCKWISE = true; 
        ///    OFFSET = 90;
        /// </summary>
        public static bool FIELD_ANGLE_CLOCKWISE = true;
        public static int FIELD_ANGLE_OFFSET = 90;

        /// <summary>
        /// Maximum allowed angle before wrapping.
        /// Usually either 360 (for 0 to 359.9) or 180 (for -179.9 to 180).
        /// </summary>
        public static int FIELD_ANGLE_WRAP = 360;

        /// <summary>
        /// Default width for field image
        /// </summary>
        public static int FIELD_IMAGE_WIDTH_DEFAULT = 30;
        /// <summary>
        /// Default height for field image
        /// </summary>
        public static int FIELD_IMAGE_HEIGHT_DEFAULT = 30;
    }
}
