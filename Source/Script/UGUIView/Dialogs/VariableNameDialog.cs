using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class VariableNameDialog : BaseDialog
    {
        [SerializeField] private Text m_InputLabel;
        [SerializeField] private InputField m_Input;

        private bool mIsRename = false;
        
        private string mOldVarName;
        public void Rename(string varName)
        {
            mOldVarName = varName;
            mIsRename = true;
            m_InputLabel.text = Blockly.Msg[MsgDefine.RENAME_VARIABLE];
        }

        protected override void OnInit()
        {
            m_InputLabel.text = Blockly.Msg[MsgDefine.NEW_VARIABLE];

            AddCloseEvent(() =>
            {
                if (mIsRename)
                    BlocklyUI.WorkspaceView.Workspace.RenameVariable(mOldVarName, m_Input.text);
                else
                    BlocklyUI.WorkspaceView.Workspace.CreateVariable(m_Input.text);
            });
        }
    }
}
