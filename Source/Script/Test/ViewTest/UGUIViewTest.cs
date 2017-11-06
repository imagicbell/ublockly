using System.Collections;
using PTGame.Blockly.UGUI;
using PTGame.Test;
using UnityEngine;

namespace PTGame.Blockly.Test
{
    [PTTestUnitInfo("Test UI")]
    public class UGUIViewTest : PTTestUnit
    {
        private bool mReadXml;
        private bool mSaveXml;
        
        [PTTestSetup]
        IEnumerator Setup()
        {
            Blockly.Dispose();
            Blockly.LoadAllBlocksFromJson();
            BlocklyUI.NewWorkspace();

            yield return 0;
        }
        
        [PTTestTeardown]
        IEnumerator Teardown()
        {
            BlocklyUI.DestroyWorkspace();
            Blockly.Dispose();
            yield return 0;
        }

        [PTTestRun]
        IEnumerator Run()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "CodeTest");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            path += "/TestUIView.xml";        
            Debug.Log(">>>> path: " + path);
            
            while (true)
            {
                if (mSaveXml)
                {
                    var dom = PTGame.Blockly.Xml.WorkspaceToDom(BlocklyUI.WorkspaceView.Workspace);
                    string text = PTGame.Blockly.Xml.DomToText(dom);
                    System.IO.File.WriteAllText(path, text);
                    
                    mSaveXml = false;
                }
                else if (mReadXml)
                {
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
                    
                    mReadXml = false;
                }
                else
                {
                    yield return 0;    
                }
            }
        }
        
        [PTTestGUI(x = 650, y = 20, width = 600, height = 500)]
        void View()
        {
            if (GUI.Button(new Rect(10, 10, 200, 100), "SaveXml"))
            {
                mSaveXml = true;
                mReadXml = false;
            }
            
            if (GUI.Button(new Rect(10, 300, 200, 100), "ReadXml"))
            {
                mSaveXml = false;
                mReadXml = true;
            }
        }
    }
}