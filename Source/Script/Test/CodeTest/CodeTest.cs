using System.Collections;
using System.Xml;
using UnityEngine;
using PTGame.Test;

namespace UBlockly.Test
{
    [PTTestUnitInfo("Test Code Generator, Interpreter")]
    public class CodeTest : PTTestUnit
    {
        private CodeName mCodeName = CodeName.CSharp;
        private Workspace mWorkspace;
        private string mCode = "";
        private string mXml = "";
        private bool mNext;

        [PTTestSetup]
        IEnumerator Setup()
        {
            Blockly.Dispose();
            Blockly.Init();

            mWorkspace = new Workspace(new Workspace.WorkspaceOptions());
            yield return 0;
        }

        [PTTestTeardown]
        IEnumerator Teardown()
        {
            mWorkspace = null;
            Blockly.Dispose();
            yield return 0;
        }

        void RunCurrentCode()
        {
            switch (mCodeName)
            {
                case CodeName.CSharp:
                    mCode = CSharp.Generator.WorkspaceToCode(mWorkspace);
                    CSharp.Interpreter.Run(mWorkspace);
                    break;
                case CodeName.Lua:
                    mCode = Lua.Generator.WorkspaceToCode(mWorkspace);
                    Lua.Interpreter.Run(mWorkspace);
                    break;
            }

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
            }
        }

        IEnumerator BeginRunCode()
        {
            CodeName lastCodeName = mCodeName;
            RunCurrentCode();
            while (!mNext)
            {
                if (lastCodeName != mCodeName)
                {
                    RunCurrentCode();
                    lastCodeName = mCodeName;
                }
                yield return 0;
            }
        }
        
        public static IEnumerator XmlToWorkspace(string xmlPath, Workspace workspace)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, xmlPath);
            string inputXml;
            if (path.Contains("://"))
            {
                WWW www = new WWW(path);
                yield return www;
                inputXml = www.text;
            }
            else
                inputXml = System.IO.File.ReadAllText(path);

            var dom = Xml.TextToDom(inputXml);
            Xml.DomToWorkspace(dom, workspace);
        }

        [PTTestRun]
        IEnumerator TestGen_Math()
        {
            mNext = false;

            mWorkspace.CreateVariable("num1");
            mWorkspace.CreateVariable("num2");
            mWorkspace.CreateVariable("math");
            
            Block varArg1 = mWorkspace.NewBlock("variables_set");
            Block varArg2 = mWorkspace.NewBlock("variables_set");
            varArg1.GetField("VAR").SetValue("num1");
            varArg2.GetField("VAR").SetValue("num2");
                  
            Block arg1 = mWorkspace.NewBlock("math_number");
            Block arg2 = mWorkspace.NewBlock("math_number");
            arg1.GetField("NUM").SetValue("3");
            arg2.GetField("NUM").SetValue("5");
            varArg1.GetInput("VALUE").Connection.Connect(arg1.OutputConnection);
            varArg2.GetInput("VALUE").Connection.Connect(arg2.OutputConnection);

            Block varGetArg1 = mWorkspace.NewBlock("variables_get");
            Block varGetArg2 = mWorkspace.NewBlock("variables_get");
            varGetArg1.GetField("VAR").SetValue("num1");
            varGetArg2.GetField("VAR").SetValue("num2");
            
            Block block = mWorkspace.NewBlock("math_arithmetic");
            block.GetInput("A").Connection.Connect(varGetArg1.OutputConnection);
            block.GetInput("B").Connection.Connect(varGetArg2.OutputConnection);
            block.GetField("OP").SetValue("MINUS");

            Block mathBlock = mWorkspace.NewBlock("variables_set");
            mathBlock.GetField("VAR").SetValue("math");
            mathBlock.GetInput("VALUE").Connection.Connect(block.OutputConnection);
            
            varArg1.NextConnection.Connect(varArg2.PreviousConnection);
            varArg2.NextConnection.Connect(mathBlock.PreviousConnection);

            Block varGetMath = mWorkspace.NewBlock("variables_get");
            varGetMath.GetField("VAR").SetValue("math");
            Block printBlock = mWorkspace.NewBlock("text_print");
            printBlock.GetInput("TEXT").Connection.Connect(varGetMath.OutputConnection);
            mathBlock.NextConnection.Connect(printBlock.PreviousConnection);

            yield return BeginRunCode();
        }

        [PTTestRun]
        IEnumerator Test_Coroutine()
        {
            mNext = false;
            mCodeName = CodeName.CSharp;

            Block time = mWorkspace.NewBlock("math_number");
            time.GetField("NUM").SetValue("3");

            Block wait = mWorkspace.NewBlock("time_wait");
            wait.GetInput("TIME").Connection.Connect(time.OutputConnection);
            wait.GetField("UNIT").SetValue("SECONDS");

            yield return BeginRunCode();
        }

        [PTTestRun]
        IEnumerator TestMutatorIfElse()
        {
            mNext = false;

            /*Block ifBlock = mWorkspace.NewBlock("controls_if");
            IfElseMutator mutator = ifBlock.Mutator as IfElseMutator;
            mutator.Mutate(1, true);

            for (int i = 0; i < 2; i++)
            {
                Block arg1 = mWorkspace.NewBlock("math_number");
                Block arg2 = mWorkspace.NewBlock("math_number");
                arg1.GetField("NUM").SetValue("1");
                arg2.GetField("NUM").SetValue("2");
                Block compareBlock = mWorkspace.NewBlock("logic_compare");
                compareBlock.GetInput("A").Connection.Connect(arg1.OutputConnection);
                compareBlock.GetInput("B").Connection.Connect(arg2.OutputConnection);

                if (i == 0)
                    compareBlock.GetField("OP").SetValue("LT");
                else 
                    compareBlock.GetField("OP").SetValue("EQ");
                
                ifBlock.GetInput("IF" + i).Connection.Connect(compareBlock.OutputConnection);
            }

            for (int i = 0; i < 3; i++)
            {
                string str = "1<2";
                if (i == 1) str = "1==2";
                else if (i == 2) str = "1>2";
                Block text = mWorkspace.NewBlock("text");
                text.GetField("TEXT").SetValue(str);

                Block printBlock = mWorkspace.NewBlock("text_print");
                printBlock.GetInput("TEXT").Connection.Connect(text.OutputConnection);

                if (i < 2)
                    ifBlock.GetInput("DO" + i).Connection.Connect(printBlock.PreviousConnection);
                else
                    ifBlock.GetInput("ELSE").Connection.Connect(printBlock.PreviousConnection);
            }*/
            yield return XmlToWorkspace("PTBlockly/CodeTest/TestMutator.xml", mWorkspace);
            yield return BeginRunCode();
        }

        [PTTestRun]
        IEnumerator TestLoop()
        {
            mNext = false;

            Block repeatBlock = mWorkspace.NewBlock("controls_repeat_ext");
            
            Block timeBlock = mWorkspace.NewBlock("math_number");
            timeBlock.GetField("NUM").SetValue("10");
            repeatBlock.GetInput("TIMES").Connection.Connect(timeBlock.OutputConnection);
            
            Block text = mWorkspace.NewBlock("text");
            text.GetField("TEXT").SetValue("test repeat");
            Block printBlock = mWorkspace.NewBlock("text_print");
            printBlock.GetInput("TEXT").Connection.Connect(text.OutputConnection);

            repeatBlock.GetInput("DO").Connection.Connect(printBlock.PreviousConnection);
            
            Block durationBlock = mWorkspace.NewBlock("math_number");
            durationBlock.GetField("NUM").SetValue("3");

            Block waitBlock = mWorkspace.NewBlock("time_wait");
            waitBlock.GetInput("TIME").Connection.Connect(durationBlock.OutputConnection);
            waitBlock.GetField("UNIT").SetValue("SECONDS");
            
            printBlock.NextConnection.Connect(waitBlock.PreviousConnection);
            
            Block ifBlock = mWorkspace.NewBlock("controls_if");
            Block conditionBlock = mWorkspace.NewBlock("logic_boolean");
            Block breakBlock = mWorkspace.NewBlock("controls_flow_statements");
            breakBlock.GetField("FLOW").SetValue("BREAK");
            conditionBlock.GetField("BOOL").SetValue("TRUE");
            ifBlock.GetInput("IF0").Connection.Connect(conditionBlock.OutputConnection);
            ifBlock.GetInput("DO0").Connection.Connect(breakBlock.PreviousConnection);
            
            waitBlock.NextConnection.Connect(ifBlock.PreviousConnection);
            
            yield return BeginRunCode();
        }

        private bool toggleCodeCSharp = true;
        private bool toggleCodeLua = false;
        private bool toggleXml = false;
        
        [PTTestGUI(x = 650, y = 20, width = 1200, height = 1500)]
        void View()
        {
            GUI.skin.textArea.fontSize = 25;
            GUI.skin.textArea.clipping = TextClipping.Clip;

            if (GUI.Button(new Rect(10, 10, 100, 40), "NEXT"))
            {
                mNext = true;
            }

            bool toggle = GUI.Toggle(new Rect(20, 60, 100, 40), toggleCodeCSharp, "<size=25>C#</size>");
            if (toggle != toggleCodeCSharp && toggle)
            {
                toggleCodeCSharp = true;
                toggleCodeLua = false;
                toggleXml = false;
                
                mCodeName = CodeName.CSharp;
            }
            
            toggle = GUI.Toggle(new Rect(140, 60, 100, 40), toggleCodeLua, "<size=25>Lua</size>");
            if (toggle != toggleCodeLua && toggle)
            {
                toggleCodeCSharp = false;
                toggleCodeLua = true;
                toggleXml = false;
                
                mCodeName = CodeName.Lua;
            }
            
            toggle = GUI.Toggle(new Rect(260, 60, 100, 40), toggleXml, "<size=25>XML</size>");
            if (toggle != toggleXml && toggle)
            {
                toggleCodeCSharp = false;
                toggleCodeLua = false;
                toggleXml = true;
            }

            if (toggleCodeCSharp || toggleCodeLua)
            {
                GUI.TextArea(new Rect(10, 100, 1180, 1400), mCode);
            }
            else
            {
                GUI.TextArea(new Rect(10, 100, 1180, 1400), mXml);
            }
        }
    }
}
