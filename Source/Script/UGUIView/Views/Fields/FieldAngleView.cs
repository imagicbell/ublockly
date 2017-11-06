namespace PTGame.Blockly.UGUI
{
    public class FieldAngleView : FieldInputView
    {
        protected override void OnBindModel()
        {
            m_InputField.text = "0";
        }

        protected override void RegisterTouchEvent()
        {
        }
    }
}