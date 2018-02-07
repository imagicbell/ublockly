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
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public struct FieldDropdownMenu
    {
        /// <summary>
        /// Text to display
        /// </summary>
        public string Text;
        /// <summary>
        /// Real Value behind the text
        /// </summary>
        public string Value;
    }
    
    //todo: maoling currently not support image value
    public class FieldDropdown : Field
    {
        [FieldCreator(FieldType = "field_dropdown")]
        private static FieldDropdown CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            FieldDropdown field = new FieldDropdown(fieldName);
            if (json.JsonDataContainsKey("options"))
                field.SetOptions(json["options"] as JArray);

            field.IsImage = json.JsonDataContainsKey("image") && (bool)json["image"];
            return field;
        }

        private FieldDropdownMenu[] mMenuOptions;
        protected Func<FieldDropdownMenu[]> mMenuGenerator;

        /// <summary>
        /// Language-neutral currently selected string
        /// </summary>
        protected string mValue;

        /// <summary>
        /// Class for an editable dropdown field.
        /// </summary>
        public FieldDropdown(string fieldName) : base(fieldName) {}

        /// <summary>
        /// Return a list of the options for this dropdown.
        /// </summary>
        public FieldDropdownMenu[] GetOptions()
        {
            return mMenuGenerator != null ? mMenuGenerator() : mMenuOptions;
        }
        
        /// <summary>
        /// init the dropdown menu options
        /// </summary>
        public void SetOptions(FieldDropdownMenu[] menu)
        {
            if (menu == null)
                return;

            mMenuOptions = menu;
            TrimOptions();
            this.SetValue(mMenuOptions[0].Value);
        }
        
        /// <summary>
        /// init the dropdown menu options
        /// </summary>
        /// <param name="menu">An array of options for a dropdown list</param>
        public void SetOptions(JArray menu)
        {
            if (menu == null || !menu.IsArray())
                return;
         
            mMenuOptions = new FieldDropdownMenu[menu.Count];
            for (int i = 0; i < mMenuOptions.Length; i++)
            {
                mMenuOptions[i] = new FieldDropdownMenu()
                {
                    Text = menu[i][0].ToString(),
                    Value = menu[i][1].ToString()
                };
            }
            TrimOptions();
            this.SetValue(mMenuOptions[0].Value);
        }

        /// <summary>
        /// init the dropdown menu options
        /// </summary>
        /// <param name="menu">An array of options for a dropdown list</param>
        public void SetOptions(string[,] menu)
        {
            if (menu == null || menu.GetLength(1) != 2)
                return;

            mMenuOptions = new FieldDropdownMenu[menu.GetLength(0)];
            for (int i = 0; i < mMenuOptions.Length; i++)
            {
                mMenuOptions[i] = new FieldDropdownMenu()
                {
                    Text = menu[i, 0],
                    Value = menu[i, 1]
                };
            }
            TrimOptions();
            this.SetValue(mMenuOptions[0].Value);
        }

        /// <summary>
        /// init the dropdown menu options
        /// </summary>
        /// <param name="menuGenerator">a function which generates a dropdown option list</param>
        public void SetOptions(Func<FieldDropdownMenu[]> menuGenerator)
        {
            if (menuGenerator == null)
                return;

            mMenuGenerator = menuGenerator;
            var options = menuGenerator();
            this.SetValue(options[0].Value);
        }

        /// <summary>
        /// Factor out common words in statically defined options. Create prefix and/or suffix labels.
        /// </summary>
        private void TrimOptions()
        {
            if (mMenuOptions == null) return;
            
            this.PrefixField = null;
            this.SuffixField = null;

            for (int i = 0; i < mMenuOptions.Length; i++)
            {
                mMenuOptions[i].Text = Utils.ReplaceMessageReferences(mMenuOptions[i].Text);
            }
            if (mMenuOptions.Length < 2)
                return;

            string[] strs = new string[mMenuOptions.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                strs[i] = mMenuOptions[i].Text;
            }
            
            int shortest = Utils.ShortestStringLength(strs);
            int prefixLength = Utils.CommonWordPrefix(strs);
            int suffixLength = Utils.CommonWordSuffix(strs);
            if (prefixLength == 0 && suffixLength == 0)
                return;
            if (shortest <= prefixLength + suffixLength)
                return;// One or more strings will entirely vanish if we proceed.  Abort.

            if (prefixLength > 0)
                this.PrefixField = new FieldLabel("Prefix_" + this.Name, strs[0].Substring(0, prefixLength - 1));
            if (suffixLength > 0)
                this.SuffixField = new FieldLabel("Prefix_" + this.Name, strs[0].Substring(strs[0].Length - suffixLength + 1));

            // Remove the prefix and suffix from the options.
            for (int i = 0; i < mMenuOptions.Length; i++)
            {
                var text = mMenuOptions[i].Text;
                var value = mMenuOptions[i].Value;
                mMenuOptions[i] = new FieldDropdownMenu()
                {
                    Text = text.Substring(prefixLength, text.Length - prefixLength - suffixLength),
                    Value = value,
                };
            }
        }

        public override string GetValue()
        {
            return mValue;
        }

        public override void SetValue(string newValue)
        {
            if (string.IsNullOrEmpty(newValue) || newValue.Equals(mValue))
                return;

            this.mValue = newValue;
            string newText = newValue;
            
            // Look up and display the human-readable text.
            var options = GetOptions();
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i].Value.Equals(newValue))
                {
                    newText = options[i].Text;
                    break;
                }
            }
            
            SetText(newText);            
        }

        /// <summary>
        /// Handle the selection of an item in the dropdown menu.
        /// </summary>
        public virtual void OnItemSelected(int itemIndex)
        {
            FieldDropdownMenu[] menu = GetOptions();
            string value = menu[itemIndex].Value;
            if (SourceBlock != null)
                value = this.CallValidator(value);

            SetValue(value);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
