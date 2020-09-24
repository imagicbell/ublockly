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
    public class ProcedureDefinitionMutatorDialog : BaseDialog
    {
        [Serializable]
        private class InputPrefab
        {
            public GameObject m_Prefab;
            public Button m_DeleteButton;
            public InputField m_InputField;
        }

        [SerializeField] private InputField m_NameInput;
        [SerializeField] private Text m_NameTitle;
        
        [SerializeField] private Toggle m_StatementToggle;
        [SerializeField] private Text m_StatementTitle;

        [SerializeField] private Text m_InputTitle;
        [SerializeField] private Text m_InputCount;
        [SerializeField] private Button m_InputAddButton;
        [SerializeField] private InputPrefab m_InputPrefab;

        private ProcedureDefinitionMutator mProcedureDefMutator
        {
            get { return mBlock.Mutator as ProcedureDefinitionMutator; }
        }

        private const string DEFAULT_INPUT_NAME = "x";

        private List<string> mArgumentList;
        
        private Transform mInputParent
        {
            get { return m_InputPrefab.m_Prefab.transform.parent; }
        }

        protected override void OnInit()
        {
            mArgumentList = mProcedureDefMutator.GetArgumentNameList();
            if (mArgumentList == null) 
                mArgumentList = new List<string>();

            m_NameTitle.text = I18n.Get(MsgDefine.PROCEDURES_NAME) + ": ";
            m_NameInput.text = mProcedureDefMutator.GetProcedureName();
            
            if (mProcedureDefMutator.Block.Type.Equals(Define.DEFINE_NO_RETURN_BLOCK_TYPE))
            {
                m_StatementToggle.gameObject.SetActive(false);
            }
            else
            {
                m_StatementTitle.text = I18n.Get(MsgDefine.PROCEDURES_ALLOW_STATEMENTS);
                m_StatementToggle.isOn = mProcedureDefMutator.HasStatement();
            }

            m_InputTitle.text = I18n.Get(MsgDefine.PROCEDURES_MUTATORCONTAINER_TITLE) + ": ";
            m_InputPrefab.m_InputField.placeholder.GetComponent<Text>().text = "Enter input name...";

            m_InputAddButton.onClick.AddListener(() =>
            {
                AddInput(mInputParent.childCount - 1);
            });

            for (int i = 0; i < mArgumentList.Count; i++)
            {
                AddInput(i, mArgumentList[i]);
            }
            
            AddCloseEvent(() =>
            {
                Procedure info = new Procedure(m_NameInput.text, mArgumentList, m_StatementToggle.isOn);
                mProcedureDefMutator.ProcedureDB.MutateProcedure(mProcedureDefMutator.ProcedureInfo.Name, info);
            });
        }

        private void AddInput(int index, string inputName = DEFAULT_INPUT_NAME)
        {
            //create a new input
            GameObject newInput = GameObject.Instantiate(m_InputPrefab.m_Prefab, mInputParent, false);
            newInput.SetActive(true);

            //input name
            InputField inputField = newInput.GetComponentInChildren<InputField>();
            inputField.text = inputName;
            if (mArgumentList.Count > index)
                mArgumentList[index] = inputName;
            else 
                mArgumentList.Add(inputName);                
            inputField.onValueChanged.AddListener((s) =>
            {
                //as index will change after deleting other inputs, re-compute it!
                int id = newInput.transform.GetSiblingIndex() - 1;
                mArgumentList[id] = s;
            });
            
            //delete button
            Button deleteBtn = newInput.GetComponentInChildren<Button>();
            deleteBtn.onClick.AddListener(() =>
            {
                mArgumentList.RemoveAt(index);
                GameObject.Destroy(mInputParent.GetChild(index + 1).gameObject);
                
                m_InputCount.text = mArgumentList.Count.ToString();
            });
            
            m_InputCount.text = mArgumentList.Count.ToString();
        }
    }
}
