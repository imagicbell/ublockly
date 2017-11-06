using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PTGame.Blockly.UGUI
{
    /// <summary>
    /// Deal with Workspace - XML save and load
    /// </summary>
    public class XmlView : MonoBehaviour
    {
        [SerializeField] private Button m_SaveBtn;
        [SerializeField] private Button m_LoadBtn;

        [SerializeField] private GameObject m_SavePanel;
        [SerializeField] private InputField m_SaveNameInput;
        [SerializeField] private Button m_SaveOkBtn;

        [SerializeField] private GameObject m_LoadPanel;
        [SerializeField] private RectTransform m_ScrollContent;
        [SerializeField] private GameObject m_XmlBtnPrefab;

        private string mSavePath;

        private bool mIsSavePanelShow
        {
            get { return m_SavePanel.activeInHierarchy; }
        }

        private bool mIsLoadPanelShow
        {
            get { return m_LoadPanel.activeInHierarchy; }
        }

        private void Awake()
        {
            mSavePath = System.IO.Path.Combine(Application.persistentDataPath, "XmlSave");
            if (!System.IO.Directory.Exists(mSavePath))
                System.IO.Directory.CreateDirectory(mSavePath);
            
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

        private void ShowSavePanel()
        {
            m_SavePanel.SetActive(true);
            m_LoadPanel.SetActive(false);
        }

        private void HideSavePanel()
        {
            m_SavePanel.SetActive(false);
        }

        private void ShowLoadPanel()
        {
            m_LoadPanel.SetActive(true);
            m_SavePanel.SetActive(false);

            string[] xmlFiles = Directory.GetFiles(mSavePath);
            for (int i = 0; i < xmlFiles.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(xmlFiles[i]);
                GameObject btnXml = GameObject.Instantiate(m_XmlBtnPrefab, m_ScrollContent, false);
                btnXml.SetActive(true);
                btnXml.GetComponentInChildren<Text>().text = fileName;
                btnXml.GetComponent<Button>().onClick.AddListener(() =>
                {
                    StartCoroutine(LoadXml(fileName));
                });
            }
        }

        private void HideLoadPanel()
        {
            m_LoadPanel.SetActive(false);

            for (int i = 1; i < m_ScrollContent.childCount; i++)
            {
                GameObject.Destroy(m_ScrollContent.GetChild(i).gameObject);
            }
        }

        private void SaveXml()
        {
            var dom = PTGame.Blockly.Xml.WorkspaceToDom(BlocklyUI.WorkspaceView.Workspace);
            string text = PTGame.Blockly.Xml.DomToText(dom);
            string path = mSavePath;
            if (!string.IsNullOrEmpty(m_SaveNameInput.text))
                path = System.IO.Path.Combine(path, m_SaveNameInput.text + ".xml");
            else
                path = System.IO.Path.Combine(path, "Default.xml");

            System.IO.File.WriteAllText(path, text);
            
            HideSavePanel();
        }

        IEnumerator LoadXml(string fileName)
        {
            BlocklyUI.WorkspaceView.CleanViews();

            string path = System.IO.Path.Combine(mSavePath, fileName + ".xml");
            string inputXml;
            if (path.Contains("://"))
            {
                WWW www = new WWW(path);
                yield return www;
                inputXml = www.text;
            }
            else
                inputXml = System.IO.File.ReadAllText(path);

            var dom = PTGame.Blockly.Xml.TextToDom(inputXml);
            PTGame.Blockly.Xml.DomToWorkspace(dom, BlocklyUI.WorkspaceView.Workspace);
            BlocklyUI.WorkspaceView.BuildViews();
            
            HideLoadPanel();
        }
    }
}