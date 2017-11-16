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
            return new FieldColour(fieldName, json["colour"].ToString());
        }

        private string mColor;
        
        /// <summary>
        /// Class for a colour input field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param>
        /// <param name="color">The initial colour in '#rrggbb' format.</param>
        public FieldColour(string fieldName, string color) : base(fieldName)
        {
            this.SetValue(color);
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
            mColor = newValue;
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
    }
}
