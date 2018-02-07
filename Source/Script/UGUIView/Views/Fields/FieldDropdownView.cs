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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    /* temporarily discard
    public class FieldDropdownView : FieldView
    {
        [SerializeField] protected CustomDropdown m_Dropdown;

        protected FieldDropdown mFieldDropdown
        {
            get { return mField as FieldDropdown; }
        }

        /// <summary>
        /// false: value updated from UI, need to cast event to model
        /// true: value updated from model, no need to cast event to model
        /// </summary>
        protected bool mUpdateFromModel = false;
        
        protected float mHorizontalMargin;
        
        protected override void SetComponents()
        {
            if (m_Dropdown == null)
                m_Dropdown = GetComponentInChildren<CustomDropdown>();

            mHorizontalMargin = m_Dropdown.captionText.rectTransform.offsetMin.x - m_Dropdown.captionText.rectTransform.offsetMax.x;
        }

        protected override void OnBindModel()
        {
            m_Dropdown.options.Clear();
            List<string> optionTexts = mFieldDropdown.GetOptions().Select(o => o.Text).ToList();
            m_Dropdown.AddOptions(optionTexts);
            string fieldText = mFieldDropdown.GetText();
            int option = optionTexts.FindIndex(o => o.Equals(mFieldDropdown.GetText()));
            if (option != -1)
                m_Dropdown.value = option;
            
            m_Dropdown.AddShowOptionsListener(UpdateMenuWidth);
        }

        protected override void OnUnBindModel()
        {
        }

        protected override void RegisterTouchEvent()
        {
            m_Dropdown.onValueChanged.AddListener(newOption =>
            {
                if (mUpdateFromModel)
                {
                    mUpdateFromModel = false;
                    return;
                }
                mFieldDropdown.OnItemSelected(newOption);
            });
        }

        protected override void OnValueChanged(string newValue)
        {
            int newIndex = m_Dropdown.options.FindIndex(o => o.text.Equals(newValue));
            if (newIndex < 0) return;

            if (m_Dropdown.value != newIndex)
            {
                mUpdateFromModel = true;
                m_Dropdown.value = newIndex;
            }
            UpdateLayout(XY);    
        }

        protected override Vector2 CalculateSize()
        {
            float width = m_Dropdown.captionText.CalculateTextWidth(m_Dropdown.options[m_Dropdown.value].text);
            width += mHorizontalMargin;
            
            Debug.LogFormat(">>>>> CalculateSize-Dropdown: text: {0}, width: {1}", m_Dropdown.options[m_Dropdown.value].text, width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }

        /// <summary>
        /// dynamically update the dropdown menu width according to option texts' max width
        /// </summary>
        private void UpdateMenuWidth()
        {
            string maxOption = "";
            foreach (var option in m_Dropdown.options)
            {
                if (option.text.Length > maxOption.Length)
                    maxOption = option.text;
            }

            float width = m_Dropdown.itemText.CalculateTextWidth(maxOption);
            RectTransform itemTextTrans = m_Dropdown.itemText.GetComponent<RectTransform>();
            width += itemTextTrans.offsetMin.x;
            m_Dropdown.transform.FindChild("Dropdown List").GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }*/
}
