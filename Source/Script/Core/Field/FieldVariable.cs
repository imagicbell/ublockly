
using System.Collections.Generic;

namespace UBlockly
{
    public sealed class FieldVariable : FieldDropdown
    {
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

            options.Add(new FieldDropdownMenu()
            {
                Text = Blockly.Msg[MsgDefine.RENAME_VARIABLE],
                Value = Blockly.RENAME_VARIABLE_ID
            });
            options.Add(new FieldDropdownMenu()
            {
                Text = Blockly.Msg[MsgDefine.DELETE_VARIABLE],
                Value = Blockly.DELETE_VARIABLE_ID
            });
            return options.ToArray();
        }

        public override void OnItemSelected(int itemIndex)
        {
            FieldDropdownMenu[] menu = GetOptions();
            string id = menu[itemIndex].Value;
            if (id.Equals(Blockly.RENAME_VARIABLE_ID))
            {
                // wait for UI
                return;
            }
            if (id.Equals(Blockly.DELETE_VARIABLE_ID))
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
                    if (!string.IsNullOrEmpty(text) && !mText.Equals(text))
                        SetValue(text);
                }
            }
        }
    }
}
