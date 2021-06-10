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


using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldInputView : FieldView
    {
        [SerializeField] protected InputField m_InputField;

        private FieldTextInput mFieldInput
        {
            get { return mField as FieldTextInput; }
        }

        private float mHorizontalMargin;
        
        protected override void SetComponents()
        {
            if (m_InputField == null)
                m_InputField = GetComponentInChildren<InputField>();

            mHorizontalMargin = m_InputField.textComponent.rectTransform.offsetMin.x - m_InputField.textComponent.rectTransform.offsetMax.x;
        }

        protected override void OnBindModel()
        {
            m_InputField.text = mField.GetValue();
        }

        protected override void OnUnBindModel()
        {
        }

        protected override void RegisterTouchEvent()
        {
            m_InputField.onValueChanged.AddListener(newText =>
            {
                mField.SetValue(newText);
            });
        }
        
        protected override void OnValueChanged(string newValue)
        {
            if (!string.Equals(m_InputField.text, newValue))
                m_InputField.text = newValue;
            UpdateLayout(XY);    
        }

        protected override Vector2 CalculateSize()
        {
            float width = m_InputField.textComponent.CalculateTextWidth(m_InputField.text);
            width += mHorizontalMargin;

            //Debug.LogFormat(">>>>> CalculateSize-TextInput: text: {0}, width: {1}", m_InputField.text, width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }
    }
}
