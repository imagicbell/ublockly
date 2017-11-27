using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldColorDialog : FieldDialog
    {
        [SerializeField] private GameObject m_ColorPrefab;

        private FieldColour mFieldColor
        {
            get { return mField as FieldColour; }
        }

        protected List<Toggle> mToggleColors;

        protected override void OnInit()
        {
            m_ColorPrefab.GetComponent<Toggle>().group.allowSwitchOff = true;
            
            mToggleColors = new List<Toggle>();
            string[] colorOptions = mFieldColor.GetOptions();
            for (int i = 0; i < colorOptions.Length; i++)
            {
                Color color;
                if (!ColorUtility.TryParseHtmlString(colorOptions[i], out color))
                    throw new Exception("TryParseHtmlString failed. Please make sure the right color hex string is given.");

                GameObject colorObj = GameObject.Instantiate(m_ColorPrefab, m_ColorPrefab.transform.parent, false);
                colorObj.SetActive(true);
                Toggle colorToggle = colorObj.GetComponent<Toggle>();
                colorToggle.targetGraphic.color = color;
                colorToggle.isOn = colorOptions[i].ToLower().Equals(mFieldColor.GetValue().ToLower());

                mToggleColors.Add(colorToggle);
            }
            
            AddCloseEvent(() =>
            {
                for (int i = 0; i < mToggleColors.Count; i++)
                {
                    if (mToggleColors[i].isOn)
                    {
                        string colorHex = ColorUtility.ToHtmlStringRGB(mToggleColors[i].targetGraphic.color);
                        mFieldColor.SetValue("#" + colorHex);
                        break;
                    }
                }
            });
        }
    }
}
