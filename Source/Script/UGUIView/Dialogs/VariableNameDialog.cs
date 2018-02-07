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
            m_InputLabel.text = I18n.Msg[MsgDefine.RENAME_VARIABLE];
        }

        protected override void OnInit()
        {
            m_InputLabel.text = I18n.Msg[MsgDefine.NEW_VARIABLE];

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
