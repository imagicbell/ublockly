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
    }
}
