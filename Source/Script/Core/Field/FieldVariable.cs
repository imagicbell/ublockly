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


using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public sealed class FieldVariable : FieldDropdown
    {
        [FieldCreator(FieldType = "field_variable")]
        private static FieldVariable CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            var varName = json["variable"].IsString() ? Utils.ReplaceMessageReferences(json["variable"].ToString()) : "";
            return new FieldVariable(fieldName, varName);
        }
        
        /// <summary>
        /// Class for a variable's dropdown field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param>
        /// <param name="varName"> The default name for the variable. If null, a unique variable name will be geenerated.</param>
        public FieldVariable(string fieldName, string varName) : base(fieldName)
        {
            mMenuGenerator = DropdownCreate;
            mMenuGenerator();
            SetValue(string.IsNullOrEmpty(varName) ? "" : varName);
        }

        public string GetRealValue()
        {
            return mValue;
        }

        public override string GetValue()
        {
            return GetText();
        }
        
        public override void SetValue(string newValue)
        {
            string value = newValue;
            string text = newValue;

            if (SourceBlock != null)
            {
                VariableModel variable = SourceBlock.Workspace.GetVariableById(newValue);
                if (variable != null)
                {
                    text = variable.Name;
                }
                else
                {
                    variable = SourceBlock.Workspace.GetVariable(newValue);
                    if (variable != null)
                        value = variable.ID;
                }
            }

            mValue = value;
            SetText(text);
        }
        
        /// <summary>
        /// maoling: private method, please call GetOptions() in FieldDropdown instead!!!
        /// Return a sorted list of variable names for variable dropdown menus.
        /// Include a special option at the end for creating a new variable name.
        /// </summary>
        private FieldDropdownMenu[] DropdownCreate()
        {
            List<VariableModel> varModels = null;
            
            string name = GetText();
            // Don't create a new variable if there is nothing selected.
            var createSelectedVariable = !string.IsNullOrEmpty(name);
            Workspace workspace = SourceBlock != null ? SourceBlock.Workspace : null;
            if (workspace != null)
            {
                // Get a copy of the list, so that adding rename and new variable options
                // doesn't modify the workspace's list.
                varModels = workspace.GetVariablesOfType("");
                varModels.Sort(VariableModel.CompareByName);
            }
            
            if (varModels == null) 
                varModels = new List<VariableModel>();

            List<FieldDropdownMenu> options = new List<FieldDropdownMenu>();
            for (int i = 0; i < varModels.Count; i++)
            {
                options.Add(new FieldDropdownMenu()
                {
                    Text = varModels[i].Name,
                    Value = varModels[i].ID
                });
            }

            if (Define.FIELD_VARIABLE_ADD_MANIPULATION_OPTIONS)
            {
                options.Add(new FieldDropdownMenu()
                {
                    Text = I18n.Get(MsgDefine.RENAME_VARIABLE),
                    Value = MsgDefine.RENAME_VARIABLE
                });
                options.Add(new FieldDropdownMenu()
                {
                    Text = I18n.Get(MsgDefine.DELETE_VARIABLE),
                    Value = MsgDefine.DELETE_VARIABLE
                });
            }

            return options.ToArray();
        }

        public override void OnItemSelected(int itemIndex)
        {
            FieldDropdownMenu[] menu = GetOptions();
            string id = menu[itemIndex].Value;
            if (id.Equals(MsgDefine.RENAME_VARIABLE))
            {
                // wait for UI
                return;
            }
            if (id.Equals(MsgDefine.DELETE_VARIABLE))
            {
                // wait for UI
                return;
            }
            
            if (SourceBlock != null && SourceBlock.Workspace != null)
            {
                Workspace workspace = SourceBlock.Workspace;
                var variable = workspace.GetVariableById(id);
                if (variable != null)
                {
                    string text = this.CallValidator(variable.Name);
                    if (!string.IsNullOrEmpty(text) && !text.Equals(mText))
                        SetValue(text);
                }
            }
        }
    }
}
