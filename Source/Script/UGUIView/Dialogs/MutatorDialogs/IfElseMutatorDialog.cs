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

            m_ElseIfCountTitle.text = I18n.Get(MsgDefine.CONTROLS_IF_MSG_ELSEIF);
            m_HasElseTitle.text = I18n.Get(MsgDefine.CONTROLS_IF_MSG_ELSE);
            
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
