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
    public sealed class FieldCheckbox : Field
    {
        [FieldCreator(FieldType = "field_checkbox")]
        private static FieldCheckbox CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            return new FieldCheckbox(fieldName, json["checked"] != null && json["checked"].ToString().ToUpper() == "TRUE" ? "TRUE" : "FALSE");
        }
        
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
            newValue = newValue.ToUpper();
            if (newValue != "TRUE" && newValue != "FALSE")
                return;

            base.SetValue(newValue);
        }
    }
}
