using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class IfElseMutatorDialog : BaseDialog
    {
        [SerializeField] private Slider m_ElseIfCountSlider;
        [SerializeField] private Text m_ElseIfCountText;
        [SerializeField] private Text m_ElseIfCountTitle;
        [SerializeField] private Toggle m_HasElseToggle;
        [SerializeField] private Text m_HasElseTitle;

        private IfElseMutator mIfElseMutator
        {
            get { return mBlock.Mutator as IfElseMutator; }
        }

        protected override void OnInit()
        {
            m_ElseIfCountSlider.value = mIfElseMutator.ElseIfCount;
            m_ElseIfCountText.text = mIfElseMutator.ElseIfCount.ToString();
            m_HasElseToggle.isOn = mIfElseMutator.HasElse;

            m_ElseIfCountTitle.text = Blockly.Msg[MsgDefine.CONTROLS_IF_MSG_ELSEIF];
            m_HasElseTitle.text = Blockly.Msg[MsgDefine.CONTROLS_IF_MSG_THEN];
            
            AddCloseEvent(() =>
            {
                mIfElseMutator.Mutate((int) m_ElseIfCountSlider.value, m_HasElseToggle.isOn);
            });
            
            m_ElseIfCountSlider.onValueChanged.AddListener((value) =>
            {
                m_ElseIfCountText.text = ((int) value).ToString();
            });
        }
    }
}
