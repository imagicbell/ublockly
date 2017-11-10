using System.Collections.Generic;

namespace UBlockly
{
    /**
     * @fileoverview Blockly constants.
     * @author fenichel@google.com (Rachel Fenichel)
     */
    public partial class Blockly
    {

        /// <summary>
        /// NaN
        /// </summary>
        public const int INFINITY = -1;
        
        /**
         * Number of pixels the mouse must move before a drag starts.
         */
        public const int DRAG_RADIUS = 5;

        /**
         * Number of pixels the mouse must move before a drag/scroll starts from the
         * flyout.  Because the drag-intention is determined when this is reached, it is
         * larger than Blockly.DRAG_RADIUS so that the drag-direction is clearer.
         */
        public const int FLYOUT_DRAG_RADIUS = 10;

        /// <summary>
        /// Delay in ms between trigger and bumping unconnected block out of alinment
        /// </summary>
        public const int BUMP_DELAY = 250;

        /// <summary>
        /// Number of characters to truncate a collapsed block to
        /// </summary>
        public const int COLLAPSE_CHARS = 30;

        /// <summary>
        /// Length in ms for a touch to become a long press.
        /// </summary>
        public const int LONGPRESS = 750;

        /// <summary>
        /// Prevent a sound from playing if another sound preceded it within this many
        /// milliseconds.
        /// </summary>
        public const int SOUND_LIMIT = 100;

        /// <summary>
        /// When dragging a block out of a stack,split the stack in two (true),or drag
        /// out the block healing the stack (false).
        /// </summary>
        public const bool DRAG_STACK = true;

        /// <summary>
        /// The richness of block colours,regardless of the hue.
        /// Must be in the range of 0 (inclusive) to 1 (exclusive).
        /// </summary>
        public const float HSV_SATURATION = 0.45f;

        /// <summary>
        /// The intensity of block colours,regardless of the hue.
        /// Must be in the range of 0 (inclusive) to 1 (exclusive).
        /// </summary>
        public const float HSV_VALUE = 0.65f;
        
        public struct SpriteIcon
        {
            public int Width;
            public int Height;
            public string Url;
        }
        
        /// <summary>
        /// SPrited icons and images
        /// </summary>
        public SpriteIcon SPRITE = new SpriteIcon()
        {
            Width = 96,
            Height = 124,
            Url = "sprites.png"
        };

        /// <summary>
        /// Required name space for SVG elements.
        /// @const
        /// </summary>
        public const string SVG_NS = "http://www.w3.org/2000/svg";

        /// <summary>
        /// ENUM for a right_facing value input. E.g. 'set item to' or 'return'.
        /// @const
        /// </summary>
        public const string HTML_NS = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// ENUM for a right-facing value input. E.g. 'set item to' or 'return'.
        /// @const
        /// </summary>
        public const int INPUT_VALUE = 1;

        /// <summary>
        /// ENUM for a left-facing value output. E.g. 'random fraction'.
        /// @const
        /// </summary>
        public const int OUTPUT_VALUE = 2;

        /// <summary>
        /// ENUM for a down-facing block stack. E.g. 'if-do' or 'else'.
        /// @const
        /// </summary>
        public const int NEXT_STATEMENT = 3;

        /// <summary>
        /// ENUM for an up-facing block stack. E.g. 'break out of loop'.
        /// @const
        /// </summary>
        public const int PREVIOUS_STATEMENT = 4;

        /// <summary>
        /// ENUM for an dummy input. Used to add field(s) with no input.
        /// @const
        /// </summary>
        public const int DUMMY_INPUT = 5;

        /// <summary>
        /// ENUM for left alignment.
        /// @const
        /// </summary>
        public const int ALIGN_LEFT = -1;

        /// <summary>
        /// Enum for center alignment.
        /// @const
        /// </summary>
        public const int ALIGN_CENTER = 0;

        /// <summary>
        /// Enum for right alignment.
        /// @const
        /// </summary>
        public const int ALIGH_RIGHT = 1;

        /// <summary>
        /// Enum for no drag operation.
        /// @const
        /// </summary>
        public const int DRAG_NONE = 0;

        /// <summary>
        /// ENUM for inside the stickly DRAG_RADIUS.
        /// @const
        /// </summary>
        public const int DRAG_STICKY = 1;

        /// <summary>
        /// ENUM for inside the non-sticky DRAG_RADIUS,for differentiating between
        /// clicks and drags.
        /// @const
        /// </summary>
        public const int DRAG_BEGIN = 1;

        /// <summary>
        /// ENUM for freely draggable (outside the DRAG_RADIUS,if one applies).
        /// @const
        /// </summary>
        public const int DRAG_FREE = 2;

        /// <summary>
        /// Lookup table from determining the opposite type of a connection.
        /// @const
        /// </summary>
        public static Dictionary<int, int> OPPOSITE_TYPE = new Dictionary<int, int>()
        {
            {INPUT_VALUE, OUTPUT_VALUE},
            {OUTPUT_VALUE,INPUT_VALUE},
            {NEXT_STATEMENT,PREVIOUS_STATEMENT},
            {PREVIOUS_STATEMENT,NEXT_STATEMENT}
        };

        /// <summary>
        /// ENUM for toolbox and flyout at top of screen.
        /// @const
        /// </summary>
        public const int TOOLBOX_AT_TOP = 0;

        /// <summary>
        /// ENUM for toolbox and flyout at bottom of screen.
        /// @const
        /// </summary>
        public const int TOOLBOX_AT_BOTTOM = 1;

        /// <summary>
        /// ENUM for toolbox and flyout at left of screen.
        /// @const
        /// </summary>
        public const int TOOLBOX_AT_LEFT = 2;

        /// <summary>
        /// ENUM for toolbox and flyout at right of screen.
        /// @const
        /// </summary>
        public const int TOOLBOX_AT_RIGHT = 3;


        /// <summary>
        /// ENUM representing that an event is not in any delete areas.
        /// Null for backwards compatibility reasons.
        /// @const
        /// </summary>
        public const string DELETE_AREA_NONE = null;

        /// <summary>
        /// ENUM representing that an event is in the delete area of the trash can.
        /// @const
        /// </summary>
        public const int DELETE_AREA_TRASH = 1;

        /// <summary>
        /// ENUM representing that an event is in the delete area of the toolbox or 
        /// flyout.
        /// @const
        /// </summary>
        public const int DELETE_AREA_TOOLBOX = 2;

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

        /// <summary>
        /// String for use in the dropdown created in field_variable.
        /// This string indicates that this option in the dropdown is 'Rename
        /// variable...' and if selected,should trigger the prompt to rename a variable.
        /// @const {string}
        /// </summary>
        public const string RENAME_VARIABLE_ID = "RENAME_VARIABLE_ID";

        /// <summary>
        /// String for use in the dropdown created in field_variable.
        /// This string indicates that this option in the dropdown is 'Delete tht "%1"
        /// variable' and if selected,should trigger the prompt to delete a variable.
        /// @const {string}
        /// </summary>
        public const string DELETE_VARIABLE_ID = "DELETE_VARIABLE_ID";
        
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
    }
}
