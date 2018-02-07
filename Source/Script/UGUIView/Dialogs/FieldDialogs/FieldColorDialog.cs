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
