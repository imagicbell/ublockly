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