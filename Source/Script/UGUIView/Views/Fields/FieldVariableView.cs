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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldVariableView : FieldView
    {
        [SerializeField] protected Text m_Label;
        [SerializeField] protected Button m_BtnLabel;
        [SerializeField] protected Button m_BtnSelect;
        [SerializeField] protected Button m_BtnRename;
        [SerializeField] protected Button m_BtnDelete;
        
        protected FieldDropdown mFieldVar
        {
            get { return mField as FieldDropdown; }
        }
        
        protected float mHorizontalMargin;
        protected GameObject mMenuGroup;
        
        private MemorySafeVariableObserver mObserver;
        
        protected override void SetComponents()
        {
            if (m_BtnLabel == null)
            {
                m_BtnLabel = GetComponentInChildren<Button>(true);
                m_Label = m_BtnLabel.GetComponentInChildren<Text>(true);
            }

            mMenuGroup = GetComponentInChildren<VerticalLayoutGroup>(true).gameObject;
            if (m_BtnSelect == null)
            {
                Button[] buttons = mMenuGroup.GetComponentsInChildren<Button>(true);
                m_BtnSelect = buttons[0];
                m_BtnRename = buttons[1];
                m_BtnDelete = buttons[2];
            }
            mMenuGroup.SetActive(false);

            mHorizontalMargin = Mathf.Abs(m_Label.rectTransform.offsetMin.x) + Math.Abs(m_Label.rectTransform.offsetMax.x);
        }

        protected override void OnBindModel()
        {
            m_Label.text = mFieldVar.GetText();
            m_BtnSelect.GetComponentInChildren<Text>().text = I18n.Msg[MsgDefine.SELECT_VARIABLE];
            m_BtnRename.GetComponentInChildren<Text>().text = I18n.Msg[MsgDefine.RENAME_VARIABLE];
            m_BtnDelete.GetComponentInChildren<Text>().text = I18n.Msg[MsgDefine.DELETE_VARIABLE];
            UpdateMenuWidth();
            
            mObserver = new MemorySafeVariableObserver(this);
            mField.SourceBlock.Workspace.VariableMap.AddObserver(mObserver);
        }

        protected override void OnUnBindModel()
        {
            BlocklyUI.WorkspaceView.Workspace.VariableMap.RemoveObserver(mObserver);
        }

        protected override void RegisterTouchEvent()
        {
            m_BtnLabel.onClick.AddListener(() =>
            {
                mMenuGroup.SetActive(true);
            });

            m_BtnSelect.onClick.AddListener(() =>
            {
                mMenuGroup.SetActive(false);
                DialogFactory.CreateFieldDialog<FieldDropdownDialog>(mField);
            });

            m_BtnRename.onClick.AddListener(() =>
            {
                mMenuGroup.SetActive(false);
                //pop a rename panel
                VariableNameDialog dialog = DialogFactory.CreateDialog("variable_name") as VariableNameDialog;
                dialog.Rename(m_Label.text);
            });

            m_BtnDelete.onClick.AddListener(() =>
            {
                mMenuGroup.SetActive(false);
                mField.SourceBlock.Workspace.DeleteVariable(m_Label.text);
            });
        }

        protected override void OnValueChanged(string newValue)
        {
            m_Label.text = newValue;
            UpdateLayout(XY);
        }
        
        protected override Vector2 CalculateSize()
        {
            float width = m_Label.CalculateTextWidth(m_Label.text);
            width += mHorizontalMargin;
            
            Debug.LogFormat(">>>>> CalculateSize-Variable: text: {0}, width: {1}", m_Label.text, width);
            return new Vector2(width, BlockViewSettings.Get().ContentHeight);
        }
        
        /// <summary>
        /// dynamically update the dropdown menu width according to option texts' max width
        /// </summary>
        private void UpdateMenuWidth()
        {
            string maxOption = I18n.Msg[MsgDefine.SELECT_VARIABLE];
            if (maxOption.Length < I18n.Msg[MsgDefine.RENAME_VARIABLE].Length)
                maxOption = I18n.Msg[MsgDefine.RENAME_VARIABLE];
            if (maxOption.Length < I18n.Msg[MsgDefine.DELETE_VARIABLE].Length)
                maxOption = I18n.Msg[MsgDefine.DELETE_VARIABLE];

            Text texCom = m_BtnSelect.GetComponentInChildren<Text>();
            float width = texCom.CalculateTextWidth(maxOption);
            width += texCom.rectTransform.offsetMin.x*2;
            mMenuGroup.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private void OnVariableUpdate(VariableUpdateData updateData)
        {
            string oldValue = m_Label.text;
            bool updateThis = updateData.VarName.Equals(oldValue);
            
            switch (updateData.Type)
            {
                case VariableUpdateData.Delete:
                {
                    if (updateThis)
                    {
                        // dispose the block
                        if (!mSourceBlockView.InToolbox)
                            mSourceBlockView.Dispose();
                    }
                    break;
                }
                case VariableUpdateData.Rename:
                {
                    if (updateThis)
                    {
                        if (!mSourceBlockView.InToolbox ||
                            BlocklyUI.WorkspaceView.Toolbox.GetCategoryNameOfBlockView(mSourceBlockView) == Define.VARIABLE_CATEGORY_NAME)
                        {
                            m_Label.text = updateData.NewVarName;
                            UpdateLayout(XY);
                        }
                    }
                    break;
                }
            }
        }
        
        private class MemorySafeVariableObserver : IObserver<VariableUpdateData>
        {
            private FieldVariableView mViewRef;

            public MemorySafeVariableObserver(FieldVariableView viewRef)
            {
                mViewRef = viewRef;
            }
            
            public void OnUpdated(object variableMap, VariableUpdateData args)
            {
                if (mViewRef == null || mViewRef.ViewTransform == null)
                    ((VariableMap) variableMap).RemoveObserver(this);
                else
                    mViewRef.OnVariableUpdate(args);
            }
        }
    }
    
    
    /* old
    public class FieldVariableView : FieldDropdownView
    {
        private MemorySafeVariableObserver mObserver;
        
        protected override void OnBindModel()
        {
            base.OnBindModel();
            mObserver = new MemorySafeVariableObserver(this);
            mField.SourceBlock.Workspace.VariableMap.AddObserver(mObserver);
        }

        protected override void OnUnBindModel()
        {
            base.OnUnBindModel();
            BlocklyUI.WorkspaceView.Workspace.VariableMap.RemoveObserver(mObserver);
        }

        protected override void RegisterTouchEvent()
        {
            m_Dropdown.onValueChanged.AddListener(newOption =>
            {
                if (mUpdateFromModel)
                {
                    mUpdateFromModel = false;
                    return;
                }
                
                if (newOption < m_Dropdown.options.Count - 2)
                {
                    mFieldDropdown.OnItemSelected(newOption);
                }
                else
                {
                    m_Dropdown.captionText.text = mFieldDropdown.GetText();
                    
                    string text = m_Dropdown.options[newOption].text;
                    if (text.Equals(I18n.Msg[MsgDefine.RENAME_VARIABLE]))
                    {
                        //pop a rename panel
                        VariableNameDialog dialog = DialogFactory.CreateDialog("variable_name") as VariableNameDialog;
                        dialog.Rename(mFieldDropdown.GetText());
                    }
                    else if (text.Equals(I18n.Msg[MsgDefine.DELETE_VARIABLE]))
                    {
                        mField.SourceBlock.Workspace.DeleteVariable(mFieldDropdown.GetText());
                    }
                }
            });
        }

        private void OnVariableUpdate(VariableUpdateData updateData)
        {
            string oldValue = m_Dropdown.captionText.text;
            int oldOption = m_Dropdown.options.FindIndex(o => o.text.Equals(oldValue));
            bool updateThis = updateData.VarName.Equals(oldValue);
            
            // recalculate the menu options
            // !!! can't use mFieldDropdown.GetOptions() directly, because the VariableMap value has not changed before this event
            switch (updateData.Type)
            {
                case VariableUpdateData.Create:
                {
                    //remove the reserved options(rename, delete) first
                    Dropdown.OptionData reserved1 = m_Dropdown.options[m_Dropdown.options.Count - 2];
                    Dropdown.OptionData reserved2 = m_Dropdown.options[m_Dropdown.options.Count - 1];
                    m_Dropdown.options.RemoveRange(m_Dropdown.options.Count - 2, 2);
                    
                    m_Dropdown.options.Add(new Dropdown.OptionData(updateData.VarName));
                    m_Dropdown.options.Sort((o1, o2) => String.CompareOrdinal(o1.text.ToLower(), o2.text.ToLower()));
                    int newOption = m_Dropdown.options.FindIndex(o => o.text.Equals(oldValue));
                    if (newOption != oldOption)
                    {
                        mUpdateFromModel = true;
                        m_Dropdown.value = newOption;
                        UpdateLayout(XY);  
                    }
                    
                    //add the reserved options in the end
                    m_Dropdown.options.Add(reserved1);
                    m_Dropdown.options.Add(reserved2);
                    break;
                }
                case VariableUpdateData.Delete:
                {
                    if (updateThis)
                    {
                        // dispose the block
                        if (!mSourceBlockView.InToolbox)
                            mSourceBlockView.Dispose();
                    }
                    else
                    {
                        int option = m_Dropdown.options.FindIndex(o => o.text.Equals(updateData.VarName));
                        m_Dropdown.options.RemoveAt(option);

                        if (oldOption > option)
                        {
                            mUpdateFromModel = true;
                            m_Dropdown.value = oldOption - 1;
                            UpdateLayout(XY);  
                        }
                    }
                    break;
                }
                case VariableUpdateData.Rename:
                {
                    Dropdown.OptionData option = m_Dropdown.options.Find(o => o.text.Equals(updateData.VarName));
                    option.text = updateData.NewVarName;
                    if (updateThis)
                    {
                        mUpdateFromModel = true;
                        m_Dropdown.value = oldOption;
                        m_Dropdown.RefreshShownValue();
                        UpdateLayout(XY);  
                    }
                    break;
                }
            }
        }
        
        private class MemorySafeVariableObserver : IObserver<VariableUpdateData>
        {
            private FieldVariableView mViewRef;

            public MemorySafeVariableObserver(FieldVariableView viewRef)
            {
                mViewRef = viewRef;
            }
            
            public void OnUpdated(object variableMap, VariableUpdateData args)
            {
                if (mViewRef == null || mViewRef.ViewTransform == null)
                    ((VariableMap) variableMap).RemoveObserver(this);
                else
                    mViewRef.OnVariableUpdate(args);
            }
        }
    }*/
}
