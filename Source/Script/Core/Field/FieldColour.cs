using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public sealed class FieldColour : Field
    {
        [FieldCreator(FieldType = "field_colour")]
        private static FieldColour CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            FieldColour field = new FieldColour(fieldName, json["colour"].ToString());
            if (json["options"] != null)
            {
                JArray options = json["options"] as JArray;
                field.mColorOptions = new string[options.Count];
                for (int i = 0; i < options.Count; i++)
                {
                    field.mColorOptions[i] = (string) options[i];
                }
            }
            return field;
        }

        private string mColor;
        private string[] mColorOptions;

        private static string[] DEFAULT_COLOR_OPTIONS =
        {
            "#FFFFFF", "#000000", "#FF0000", "#00FF00", "#0000FF", 
            "#FFEB04", "#00FFFF", "#FF00FF", "#808080", "#FF851B",
            
            //http://clrs.cc/
            "#7FDBFF", "#39CCCC", /*"#001F3F", "#85144B", "#B10DC9",*/ 
        };
        
        /// <summary>
        /// Class for a colour input field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param>
        /// <param name="color">The initial colour in '#rrggbb' format.</param>
        public FieldColour(string fieldName, string color) : base(fieldName)
        {
            mColorOptions = DEFAULT_COLOR_OPTIONS;
            mColor = color;
            //this.SetText(Field.NBSP + Field.NBSP + Field.NBSP);
        }

        /// <summary>
        /// Return the current colour.
        /// </summary>
        /// <returns>Current colour in '#rrggbb' format.</returns>
        public override string GetValue()
        {
            return mColor;
        }

        /// <summary>
        /// Set the colour.
        /// </summary>
        /// <param name="newValue">The new colour in '#rrggbb' format.</param>
        public override void SetValue(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                // No change if null.
                return;
            }
            
            var oldValue = this.GetValue();
            if (string.Equals(oldValue.ToLower(), newValue.ToLower()))
                return;
            
            mColor = newValue;
            FireUpdate(mColor);
        }

        /// <summary>
        /// Get the text from this field.  Used when the block is collapsed.
        /// </summary>
        public override string GetText()
        {
            Regex rgx = new Regex(@"/^#(.)\1(.)\2(.)\3$/");
            Match match = rgx.Match(mColor);
            if (match.Success)
                return "#" + match.Value[1] + match.Value[2] + match.Value[3];
            return mColor;
        }
        
        /// <summary>
        /// Get the color options 
        /// </summary>
        public string[] GetOptions()
        {
            return mColorOptions;
        }
    }
}
