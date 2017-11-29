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
