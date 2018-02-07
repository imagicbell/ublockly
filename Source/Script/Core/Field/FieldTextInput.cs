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
    }
}
