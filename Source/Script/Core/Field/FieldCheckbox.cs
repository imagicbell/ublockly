namespace UBlockly
{
    public sealed class FieldCheckbox : Field
    {
        /// <summary>
        /// Class for a checkbox field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param>
        /// <param name="state">The initial state of the field ('TRUE' or 'FALSE').</param>
        public FieldCheckbox(string fieldName, string state) : base(fieldName)
        {
            this.SetValue(state);
        }

        public override void SetValue(string newValue)
        {
            base.SetValue(newValue);
            //todo
        }
    }
}
