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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldNumberDialog : FieldDialog
    {
        [SerializeField] private Text m_LabelNum;
        [SerializeField] private Button m_BtnClear;
        [SerializeField] private LayoutGroup m_Group;

        private FieldNumber mFieldNumber
        {
            get { return mField as FieldNumber; }
        }

        private Dictionary<Button, int> mBtnNums;
        private Button mBtnNeg;
        private Button mBtnPoint;
        
        protected override void OnInit()
        {
            mBtnNums = new Dictionary<Button, int>();
            Button[] buttons = m_Group.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                char suffix = btn.name[btn.name.Length - 1];
                if (suffix.Equals('.'))
                    mBtnPoint = btn;
                else if (suffix.Equals('-'))
                    mBtnNeg = btn;
                else
                    mBtnNums[btn] = int.Parse(suffix.ToString());

                btn.onClick.AddListener(() => OnClickPad(btn));
            }

            if (mFieldNumber.IntOnly)
                mBtnPoint.gameObject.SetActive(false);
            
            ClearNum();
            m_BtnClear.onClick.AddListener(ClearNum);
            
            AddCloseEvent(() =>
            {
                if (!string.IsNullOrEmpty(m_LabelNum.text))
                    mField.SetValue(m_LabelNum.text);
            });
        }

        private void ClearNum()
        {
            m_LabelNum.text = "";
        }
        
        private void OnClickPad(Button btn)
        {
            if (m_LabelNum.text == null)
                m_LabelNum.text = "";

            if (btn == mBtnNeg)
            {
                m_LabelNum.text = "-" + m_LabelNum.text;
            }
            else if (btn == mBtnPoint)
            {
                if (!m_LabelNum.text.Contains("."))
                    m_LabelNum.text += ".";
            }
            else
            {
                m_LabelNum.text += mBtnNums[btn];
            }
        }
    }
}
