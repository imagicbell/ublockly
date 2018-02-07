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
    public class FieldColorView : FieldView
    {
        [SerializeField] protected RawImage m_Image;
        [SerializeField] protected Button m_Button;
        
        protected override void SetComponents()
        {
            if (m_Button == null)
            {
                m_Button = GetComponentInChildren<Button>(true);
                for (int i = 0; i < m_Button.transform.childCount; i++)
                {
                    RawImage image = m_Button.transform.GetChild(i).GetComponent<RawImage>();
                    if (image != null)
                        m_Image = image;
                    else 
                        m_Button.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        protected override void OnBindModel()
        {
            m_Image.gameObject.SetActive(true);
            
            Color color;
            ColorUtility.TryParseHtmlString(mField.GetValue(), out color);
            m_Image.color = color;
        }

        protected override void OnUnBindModel()
        {
        }

        protected override void RegisterTouchEvent()
        {
            m_Button.onClick.AddListener(() =>
            {
                //open corresponding settings dialog
                DialogFactory.CreateFieldDialog(mField);
            });
        }

        protected override void OnValueChanged(string newValue)
        {
            Color color;
            ColorUtility.TryParseHtmlString(newValue, out color);
            m_Image.color = color;
            
            //no need to update layout, because its size is unchanged
        }
        
        protected override Vector2 CalculateSize()
        {
            //size is unchanged
            return new Vector2(BlockViewSettings.Get().ColorFieldWidth, BlockViewSettings.Get().ContentHeight);
        }
    }
}
