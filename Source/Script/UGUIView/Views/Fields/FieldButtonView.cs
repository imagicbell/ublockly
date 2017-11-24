using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    /// <summary>
    /// Field view contains a button, which trigger a popup settings dialog
    /// The content of button allows text or image.
    /// </summary>
    public class FieldButtonView : FieldView
    {
        [SerializeField] protected Text m_Label;
        [SerializeField] protected Image m_Image;
        [SerializeField] protected Button m_Button;
        
        protected float mHorizontalMargin;
        
        protected override void SetComponents()
        {
            if (m_Button == null)
            {
                m_Button = GetComponentInChildren<Button>(true);
                for (int i = 0; i < m_Button.transform.childCount; i++)
                {
                    Text text = m_Button.transform.GetChild(i).GetComponent<Text>();
                    if (text != null)
                        m_Label = text;
                    else
                        m_Image = m_Button.transform.GetChild(i).GetComponent<Image>();
                }
            }
        }

        protected override void OnBindModel()
        {
            if (!mField.IsImage)
            {
                m_Image.gameObject.SetActive(false);
                m_Label.gameObject.SetActive(true);

                m_Label.text = mField.GetText();
                
                mHorizontalMargin = Mathf.Abs(m_Label.rectTransform.offsetMin.x) + Mathf.Abs(m_Label.rectTransform.offsetMax.x);
            }
            else
            {
                m_Image.gameObject.SetActive(true);
                m_Label.gameObject.SetActive(false);
                
                //todo: load image
                
                mHorizontalMargin = Mathf.Abs(m_Image.rectTransform.offsetMin.x) + Mathf.Abs(m_Image.rectTransform.offsetMax.x);
            }
        }

        protected override void OnUnBindModel()
        {
            if (mField.IsImage)
            {
                //todo: unload image
            }
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
            if (!mField.IsImage)
            {
                m_Label.text = newValue;
            }
            else
            {
                //todo: unload old and load new 
            }
            UpdateLayout(XY);
        }
        
        protected override Vector2 CalculateSize()
        {
            float width;
            if (!mField.IsImage)
            {
                width = m_Label.CalculateTextWidth(m_Label.text);
            }
            else
            {
                width = m_Image.mainTexture.width;
            }
            width += mHorizontalMargin;
            Debug.LogFormat(">>>>> CalculateSize-Button: text: {0}, width: {1}", m_Label.text, width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }
    }
}