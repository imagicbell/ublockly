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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class FieldCreatorAttribute : Attribute
    {
        [Description("mark factory method for block fields")]
        public FieldCreatorAttribute() {}
        
        /// <summary>
        /// type of field, which is the same with that defined in json definition
        /// </summary>
        public string FieldType { get; set; }
    }
    
    public static class FieldFactory
    {
        private static Dictionary<string, MethodInfo> mFieldDict;
        
        /// <summary>
        /// create field from json object
        /// </summary>
        public static Field CreateFromJson(JObject json)
        {
            if (mFieldDict == null)
            {
                mFieldDict = new Dictionary<string, MethodInfo>();
                Assembly assem = Assembly.GetAssembly(typeof(Field));
                foreach (Type type in assem.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(Field)))
                    {
                        MethodInfo methodInfo = type.GetMethod("CreateFromJson", BindingFlags.Static | BindingFlags.NonPublic);
                        if (methodInfo == null)
                            throw new Exception(string.Format(
                                "There is no static function \"CreateFromJson\" for creating field in class {0}. Please add one",
                                type));

                        var attrs = methodInfo.GetCustomAttributes(typeof(FieldCreatorAttribute), false);
                        if (attrs.Length == 0)
                            throw new Exception(string.Format(
                                "You should add a \"FieldCreatorAttribute\" to static method \"CreateFromJson\" in class {0}.",
                                type));
                        mFieldDict[((FieldCreatorAttribute) attrs[0]).FieldType] = methodInfo;
                    }
                }
            }
            
            string fieldType = json["type"].ToString();
            MethodInfo fieldCreator;
            if (!mFieldDict.TryGetValue(fieldType, out fieldCreator))
                throw new Exception(string.Format(
                    "There is no static method \"CreateFromJson(JObject json)\" for type: \"{0}\". " +
                    "You should add one in the corresponding field class, and don't forget to add a \"FieldCreatorAttribute\" to the method.",
                    fieldType));

            return fieldCreator.Invoke(null, new object[] {json}) as Field;
        }
        
        /*/// <summary>
        /// create field from json object
        /// </summary>
        public static Field CreateFromJson(JObject json)
        {
            string fieldType = json["type"].ToString();
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            Field field = null;
            switch (fieldType)
            {
                case "field_label":
                {
                    var text = json["text"].IsString() ? Utils.ReplaceMessageReferences(json["text"].ToString()) : "";
                    field = new FieldLabel(fieldName, text);
                    break;
                }
                case "field_input":
                {
                    var text = json["text"].IsString() ? Utils.ReplaceMessageReferences(json["text"].ToString()) : "";
                    field = new FieldTextInput(fieldName, text);
                    break;
                }
                case "field_angle":
                {
                    field = new FieldAngle(fieldName, json["angle"].ToString());
                    break;
                }
                case "field_checkbox":
                {
                    field = new FieldCheckbox(fieldName, json["checked"] != null ? "TRUE" : "FALSE");
                    break;
                }
                case "field_colour":
                {
                    field = new FieldColour(fieldName, json["colour"].ToString());
                    break;
                }
                case "field_variable":
                {
                    var varName = json["variable"].IsString() ? Utils.ReplaceMessageReferences(json["variable"].ToString()) : "";
                    field = new FieldVariable(fieldName, varName);
                    break;
                }
                case "field_dropdown":
                {
                    field = new FieldDropdown(fieldName);
                    if (json.JsonDataContainsKey("options"))
                        ((FieldDropdown) field).SetOptions(json["options"] as JArray);
                    break;
                }
                case "field_number":
                {
                    field = new FieldNumber(fieldName,
                                            json["value"].ToString(), 
                                            json["min"] == null ? null : json["min"].ToString(),
                                            json["max"] == null ? null : json["max"].ToString());
                    break;
                }
                case "field_image":
                {
                    //todo: 
                    break;
                }
                case "field_date":
                {
                    //todo:
                    break;
                }
            }

            return field;
        }*/
    }
}
