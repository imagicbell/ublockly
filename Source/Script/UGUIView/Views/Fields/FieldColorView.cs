using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldColorView : FieldView
    {
        [SerializeField] protected Image m_Image;
        [SerializeField] protected Button m_Button;
        
        protected override void SetComponents()
        {
            if (m_Button == null)
            {
                m_Button = GetComponentInChildren<Button>(true);
                for (int i = 0; i < m_Button.transform.childCount; i++)
                {
                    Image image = m_Button.transform.GetChild(i).GetComponent<Image>();
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
            return ValidateSize(new Vector2(BlockViewSettings.Get().ColorFieldWidth, BlockViewSettings.Get().ContentHeight));
        }
    }
}