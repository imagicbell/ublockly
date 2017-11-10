using System.Collections;
using System.IO;
using System.Xml;
using UBlockly.Test;
using PTGame.Test;
using UnityEngine;

namespace UBlockly
{
    [PTTestUnitInfo("C# Test Code Generator, Interpreter")]
    public class CodeTestCSharp : PTTestUnit
    {
        private Workspace mWorkspace;
        private string mCode = "";
        private string mXml = "";

        private string mInputXml = null;

        private string mTestUnit;
        
        [PTTestSetup]
        IEnumerator Setup()
        {
            Blockly.Dispose();
            Blockly.LoadAllBlocksFromJson();

            mTestUnit = "math";

            if (!Directory.Exists(Application.dataPath + "/../PersistentPath/PTBlockly"))
                Directory.CreateDirectory(Application.dataPath + "/../PersistentPath/PTBlockly");
            
            yield return 0;
        }

        [PTTestTeardown]
        IEnumerator Teardown()
        {
            mWorkspace = null;
            Blockly.Dispose();
            yield return 0;
        }
        
        [PTTestRun]
        IEnumerator Run()
        {
            string lastTestUnit = mTestUnit;
            while (true)
            {
                if (string.IsNullOrEmpty(mCode) || mTestUnit != lastTestUnit)
                {
                    lastTestUnit = mTestUnit;
                    mWorkspace = new Workspace(new Workspace.WorkspaceOptions());
                    yield return CodeTest.XmlToWorkspace(string.Format("PTBlockly/CodeTest/{0}.xml", mTestUnit), mWorkspace);
                    
                    mCode = CSharp.Generator.WorkspaceToCode(mWorkspace);
                    File.WriteAllText(string.Format(Application.dataPath + "/../PersistentPath/PTBlockly/{0}.cs", mTestUnit), mCode);
                    
                    XmlNode node = Xml.WorkspaceToDom(mWorkspace);
                    using (var sw = new System.IO.StringWriter())
                    {
                        using (var xw = new System.Xml.XmlTextWriter(sw))
                        {
                            xw.Formatting = System.Xml.Formatting.Indented;
                            xw.Indentation = 4;
                            node.WriteContentTo(xw);
                        }
                        mXml = sw.ToString();
                        
                        File.WriteAllText(string.Format(Application.dataPath + "/../PersistentPath/PTBlockly/{0}.xml", mTestUnit), mXml);
                    }
                }
                yield return 0;
            }
        }
        
        private bool toggleCode = true;
        private bool toggleXml = false;
        
        [PTTestGUI(x = 650, y = 20, width = 1200, height = 1500)]
        void View()
        {
            GUI.skin.textArea.fontSize = 25;
            GUI.skin.textArea.clipping = TextClipping.Clip;
            GUI.skin.button.fontSize = 25;
            
            bool toggle = GUI.Toggle(new Rect(20, 10, 100, 40), toggleCode, "C#");
            if (toggle != toggleCode && toggle)
            {
                toggleCode = true;
                toggleXml = false;
            }
            
            toggle = GUI.Toggle(new Rect(140, 10, 100, 40), toggleXml, "XML");
            if (toggle != toggleXml && toggle)
            {
                toggleCode = false;
                toggleXml = true;
            }

            if (GUI.Button(new Rect(260, 10, 100, 40), "Run"))
            {
                CSharp.Interpreter.Run(mWorkspace);
            }
            
            if (toggleCode)
            {
                GUI.TextArea(new Rect(10, 110, 1180, 1400), mCode);
            }
            else
            {
                GUI.TextArea(new Rect(10, 110, 1180, 1400), mXml);
            }

            if (GUI.Button(new Rect(10, 60, 110, 40), "<color=green>math</color>"))  mTestUnit = "math";
            if (GUI.Button(new Rect(130, 60, 110, 40), "<color=green>logic</color>"))  mTestUnit = "logic";
            if (GUI.Button(new Rect(250, 60, 110, 40), "<color=green>text</color>"))  mTestUnit = "text";
            if (GUI.Button(new Rect(370, 60, 110, 40), "<color=green>variables</color>"))  mTestUnit = "variables";
            if (GUI.Button(new Rect(490, 60, 110, 40), "<color=green>lists</color>"))  mTestUnit = "lists";
            if (GUI.Button(new Rect(610, 60, 110, 40), "<color=green>functions</color>"))  mTestUnit = "functions";
            if (GUI.Button(new Rect(730, 60, 110, 40), "<color=green>loops1</color>"))  mTestUnit = "loops1";
            if (GUI.Button(new Rect(850, 60, 110, 40), "<color=green>loops2</color>"))  mTestUnit = "loops2";
            if (GUI.Button(new Rect(970, 60, 110, 40), "<color=red>loops3</color>"))  mTestUnit = "loops3";
            if (GUI.Button(new Rect(1090, 60, 110, 40), "<color=green>colour</color>"))  mTestUnit = "colour";
        }
    }
}
