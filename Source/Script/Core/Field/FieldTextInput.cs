using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public class FieldTextInput : Field
    {
        [FieldCreator(FieldType = "field_input")]
        private static FieldTextInput CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            var text = json["text"].IsString() ? Utils.ReplaceMessageReferences(json["text"].ToString()) : "";
            return new FieldTextInput(fieldName, text);
        }
        
        /// <summary>
        /// Empty constructor for inheritance use
        /// </summary>
        protected FieldTextInput(string fieldName) : base(fieldName) {}

        /// <summary>
        /// Class for an editable text field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param>
        /// <param name="text">The default text in the field</param>
        public FieldTextInput(string fieldName, string text) : base(fieldName)
        {
            this.SetValue(text);
        }

        public override void SetValue(string newValue)
        {
            if (string.IsNullOrEmpty(newValue)) 
                return;

            if (SourceBlock != null)
            {
                string validated = CallValidator(newValue);
                if (validated != null)
                    newValue = validated;
            }
            
            base.SetValue(newValue);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
