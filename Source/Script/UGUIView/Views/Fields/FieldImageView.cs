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
    public class FieldImageView : FieldView
    {
        [SerializeField] protected RawImage m_Image;

        private FieldImage mFieldImage
        {
            get { return mField as FieldImage; }
        }

        protected override void SetComponents()
        {
            if (m_Image == null)
                m_Image = GetComponentInChildren<RawImage>();
        }

        protected override void OnBindModel()
        {
            m_Image.texture = BlockResMgr.Get().LoadTexture(mField.GetValue());
            m_Image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mFieldImage.Size.x);
            m_Image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mFieldImage.Size.y);
        }

        protected override void OnUnBindModel()
        {
            BlockResMgr.Get().UnloadTexture(mField.GetValue());
        }

        protected override void RegisterTouchEvent()
        {
        }

        protected override void OnValueChanged(string newValue)
        {
        }
        
        protected override Vector2 CalculateSize()
        {
            int width = mFieldImage.Size.x;
            //Debug.LogFormat(">>>>> CalculateSize-Image: width: {0}", width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }
    }
}