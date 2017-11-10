using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
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
                    if (text.Equals(Blockly.Msg[MsgDefine.RENAME_VARIABLE]))
                    {
                        //pop a rename panel
                        VariableNameDialog dialog = DialogFactory.CreateDialog("variable_name") as VariableNameDialog;
                        dialog.Rename(mFieldDropdown.GetText());
                    }
                    else if (text.Equals(Blockly.Msg[MsgDefine.DELETE_VARIABLE]))
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
    }
}
