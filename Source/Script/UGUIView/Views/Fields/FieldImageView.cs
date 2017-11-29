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
            Debug.LogFormat(">>>>> CalculateSize-Image: width: {0}", width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }
    }
}