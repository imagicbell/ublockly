using UnityEngine;

namespace PTGame.Blockly
{
    public sealed class FieldLabel : Field
    {
        /// <summary>
        /// Class for a non-editable field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param> 
        /// <param name="text">The initial content of the field</param>
        public FieldLabel(string fieldName, string text) : base(fieldName)
        {
            this.SetValue(text);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}