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
    public class FieldCheckboxView : FieldView
    {
        [SerializeField] protected Toggle m_Toggle;
        private float uiWidth;
            
        protected override void SetComponents()
        {
            if (m_Toggle == null)
            {
                m_Toggle = GetComponentInChildren<Toggle>(true);
                uiWidth = GetComponent<RectTransform>().rect.width;
            }
        }    

        protected override void OnBindModel()
        {
            m_Toggle.isOn = mField.GetValue() == "TRUE";
        }    

        protected override void OnUnBindModel()
        {
        }

        protected override void RegisterTouchEvent()
        {
            m_Toggle.onValueChanged.AddListener(isOn =>
            {
                mField.SetValue(isOn ? "TRUE" : "FALSE");
            });
        }

        protected override void OnValueChanged(string newValue)
        {
            bool isOn = newValue == "TRUE";
            if (m_Toggle.isOn != isOn)
                m_Toggle.isOn = isOn;
        }

        protected override Vector2 CalculateSize()
        {
            return new Vector2(uiWidth, BlockViewSettings.Get().ContentHeight);
        }
    }
}