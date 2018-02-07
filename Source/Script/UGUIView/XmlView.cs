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


using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    /// <summary>
    /// Deal with Workspace - XML save and load
    /// </summary>
    public class XmlView : MonoBehaviour
    {
        [SerializeField] protected Button m_SaveBtn;
        [SerializeField] protected Button m_LoadBtn;

        [SerializeField] protected GameObject m_SavePanel;
        [SerializeField] protected InputField m_SaveNameInput;
        [SerializeField] protected Button m_SaveOkBtn;

        [SerializeField] protected GameObject m_LoadPanel;
        [SerializeField] protected RectTransform m_ScrollContent;
        [SerializeField] protected GameObject m_XmlBtnPrefab;

        protected bool mIsSavePanelShow
        {
            get { return m_SavePanel.activeInHierarchy; }
        }

        protected bool mIsLoadPanelShow
        {
            get { return m_LoadPanel.activeInHierarchy; }
        }
        
        protected string mSavePath;

        protected string GetSavePath()
        {
            if (string.IsNullOrEmpty(mSavePath))
            {
                mSavePath = System.IO.Path.Combine(Application.persistentDataPath, "XmlSave");
                if (!System.IO.Directory.Exists(mSavePath))
                    System.IO.Directory.CreateDirectory(mSavePath);
            }
            return mSavePath;
        }

        private void Awake()
        {
            HideSavePanel();
            HideLoadPanel();

            m_SaveBtn.onClick.AddListener(() =>
            {
                if (!mIsSavePanelShow) ShowSavePanel();
                else HideSavePanel();
            });

            m_LoadBtn.onClick.AddListener(() =>
            {
                if (!mIsLoadPanelShow) ShowLoadPanel();
                else HideLoadPanel();
            });
            
            m_SaveOkBtn.onClick.AddListener(SaveXml);
        }

        protected virtual void ShowSavePanel()
        {
            m_SavePanel.SetActive(true);
            m_LoadPanel.SetActive(false);
        }

        protected virtual void HideSavePanel()
        {
            m_SavePanel.SetActive(false);
        }

        protected virtual void ShowLoadPanel()
        {
            m_LoadPanel.SetActive(true);
            m_SavePanel.SetActive(false);

            string[] xmlFiles = Directory.GetFiles(GetSavePath());
            for (int i = 0; i < xmlFiles.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(xmlFiles[i]);
                GameObject btnXml = GameObject.Instantiate(m_XmlBtnPrefab, m_ScrollContent, false);
                btnXml.SetActive(true);
                btnXml.GetComponentInChildren<Text>().text = fileName;
                btnXml.GetComponent<Button>().onClick.AddListener(() => LoadXml(fileName));
            }
        }

        protected virtual void HideLoadPanel()
        {
            m_LoadPanel.SetActive(false);

            for (int i = 1; i < m_ScrollContent.childCount; i++)
            {
                GameObject.Destroy(m_ScrollContent.GetChild(i).gameObject);
            }
        }

        protected virtual void SaveXml()
        {
            var dom = UBlockly.Xml.WorkspaceToDom(BlocklyUI.WorkspaceView.Workspace);
            string text = UBlockly.Xml.DomToText(dom);
            string path = GetSavePath();
            if (!string.IsNullOrEmpty(m_SaveNameInput.text))
                path = System.IO.Path.Combine(path, m_SaveNameInput.text + ".xml");
            else
                path = System.IO.Path.Combine(path, "Default.xml");

            System.IO.File.WriteAllText(path, text);
            
            HideSavePanel();
        }

        protected virtual void LoadXml(string fileName)
        {
            StartCoroutine(AsyncLoadXml(fileName));
        }
        
        IEnumerator AsyncLoadXml(string fileName)
        {
            BlocklyUI.WorkspaceView.CleanViews();

            string path = System.IO.Path.Combine(GetSavePath(), fileName + ".xml");
            string inputXml;
            if (path.Contains("://"))
            {
                WWW www = new WWW(path);
                yield return www;
                inputXml = www.text;
            }
            else
                inputXml = System.IO.File.ReadAllText(path);

            var dom = UBlockly.Xml.TextToDom(inputXml);
            UBlockly.Xml.DomToWorkspace(dom, BlocklyUI.WorkspaceView.Workspace);
            BlocklyUI.WorkspaceView.BuildViews();
            
            HideLoadPanel();
        }
    }
}
