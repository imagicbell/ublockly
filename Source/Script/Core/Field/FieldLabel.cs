using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    public sealed class FieldLabel : Field
    {
        [FieldCreator(FieldType = "field_label")]
        private static FieldLabel CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            var text = json["text"].IsString() ? Utils.ReplaceMessageReferences(json["text"].ToString()) : "";
            return new FieldLabel(fieldName, text);
        }
        
        /// <summary>
        /// Class for a non-editable field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param> 
        /// <param name="text">The initial content of the field</param>
        public FieldLabel(string fieldName, string text) : base(fieldName)
        {
            this.SetValue(text);
        }
    }
}
