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

            Debug.LogFormat(">>>>> CalculateSize-TextInput: text: {0}, width: {1}", m_InputField.text, width);
            return ValidateSize(new Vector2(width, BlockViewSettings.Get().ContentHeight));
        }
    }
}
