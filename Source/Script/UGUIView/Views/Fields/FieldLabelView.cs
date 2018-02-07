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
    public class FieldLabelView : FieldView
    {
        [SerializeField] protected Text m_TextUI;

        protected override void SetComponents()
        {
            if (m_TextUI == null)
                m_TextUI = GetComponentInChildren<Text>();
        }

        protected override void OnBindModel()
        {
            m_TextUI.text = mField.GetValue();
        }

        protected override void OnUnBindModel()
        {
        }

        protected override void RegisterTouchEvent()
        {
            // no touch event on labels
        }

        protected override void OnValueChanged(string newValue)
        {
        }

        protected override Vector2 CalculateSize()
        {
            int width = m_TextUI.CalculateTextWidth(m_TextUI.text);
            Debug.LogFormat(">>>>> CalculateSize-Label: text: {0}, width: {1}", m_TextUI.text, width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }
    }
}
