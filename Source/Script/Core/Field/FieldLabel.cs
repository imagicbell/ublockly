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
