namespace UBlockly
{
    public class FieldTextInput : Field
    {
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
