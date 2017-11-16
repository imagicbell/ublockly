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
                m_Image = m_Button.GetComponentInChildren<Image>(true);
                foreach (Transform child in m_Button.transform)
                {
                    child.gameObject.SetActive(child == m_Image.transform);
                }
            }
        }

        protected override void OnBindModel()
        {
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