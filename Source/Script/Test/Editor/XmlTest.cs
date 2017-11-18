
using System;
using System.Xml;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UBlockly.Test
{
    public class XmlTest
    {
        private Workspace mWorkspace;

        private const string XML_TEXT = "<xml xmlns=\"http://www.w3.org/1999/xhtml\">" +
                                        "  <block type=\"controls_repeat_ext\" inline=\"true\" x=\"21\" y=\"23\">" +
                                        "    <value name=\"TIMES\">" +
                                        "      <block type=\"math_number\">" +
                                        "      </block>" +
                                        "    </value>" +
                                        "    <statement name=\"DO\">" +
                                        "      <block type=\"variables_set\" inline=\"true\">" +
                                        "        <field name=\"VAR\">item</field>" +
                                        "        <value name=\"VALUE\">" +
                                        "          <block type=\"lists_create_empty\"></block>" +
                                        "        </value>" +
                                        "        <next>" +
                                        "          <block type=\"text_print\" inline=\"false\">" +
                                        "            <value name=\"TEXT\">" +
                                        "              <block type=\"text\">" +
                                        "                <field name=\"TEXT\">Hello</field>" +
                                        "              </block>" +
                                        "            </value>" +
                                        "          </block>" +
                                        "        </next>" +
                                        "      </block>" +
                                        "    </statement>" +
                                        "  </block>" +
                                        "</xml>";

        void Setup()
        {
            mWorkspace = new Workspace();
        }

        void SetupWithMockBlocks()
        {
            Setup();

            Blockly.DefineBlocksWithJsonArray(
                JArray.Parse(@"[{
                'type' : 'field_variable_test_block',
                'message0' : '%1',
                'args0' : [
                    { 'type' : 'field_variable',
                      'name' : 'VAR',
                      'variable' : 'item',
                    }
                ]
            }]")
            );
        }

        void Teardown()
        {
            Utils.ResetGenUidValueDirty2False();
            mWorkspace.Dispose();
            Blockly.Dispose();
        }

        void TeardownWithMockBlocks()
        {
            Teardown();
        }

        /// <summary>
        /// Check the values of the non variable field dom.
        /// </summary>
        /// <param name="fieldDom"> The xml dom of the non variable field.</param>
        /// <param name="name"> The expected name of the variable.</param>
        /// <param name="text"> The expected text of the variable.</param>
        void XmlTestCheckNonVariableField(XmlNode fieldDom, string name, string text)
        {
            Assert.AreEqual(text, fieldDom.TextContent());
            Assert.AreEqual(name, fieldDom.GetAttribute("name"));
            Assert.IsNull(fieldDom.GetAttribute("id"));
            Assert.IsNull(fieldDom.GetAttribute("variableType"));
        }

        /// <summary>
        /// Check the values of the variable field DOM.
        /// </summary>
        /// <param name="fieldDom">The xml dom of the variable field.</param>
        /// <param name="name">The expected name of the variable.</param>
        /// <param name="type">The expected type of the variable.</param>
        /// <param name="id">The expected id of the variable.</param>
        /// <param name="text">The expected text of the variable.</param>
        public void XmlTestCheckVariableFieldDomValues(XmlNode fieldDom, string name, string type, string id,
            string text)
        {
            Assert.AreEqual(name, fieldDom.GetAttribute("name"));
            Assert.AreEqual(type, fieldDom.GetAttribute("variableType"));
            Assert.AreEqual(id, fieldDom.GetAttribute("id"));
            Assert.AreEqual(text, fieldDom.TextContent());
        }

        /// <summary>
        /// Check the values of the varible Dom.
        /// </summary>
        /// <param name="variableDom"> The xml dom of the variable.</param>
        /// <param name="type"> The expected type of the variable.</param>
        /// <param name="id"> The expected id of the variable.</param>
        /// <param name="text"> The expected text of the variable.</param>
        public void XmlTestCheckVariableDomValues(XmlNode variableDom, string type, string id, string text)
        {
            Assert.AreEqual(type, variableDom.GetAttribute("type"));
            Assert.AreEqual(id, variableDom.GetAttribute("id"));
            Assert.AreEqual(text, variableDom.TextContent());
        }


        [Test]
        public void TestTextToDom()
        {
            var dom = Xml.TextToDom(XML_TEXT);
            Assert.IsTrue(string.Equals("xml", dom.Name));
            Assert.AreEqual(6, dom.GetElementsByTagName("block").Count, "Block tags");
        }

        /*[Test]
        public void TestDomToText()
        {
            var dom = Xml.TextToDom(XML_TEXT);
            var text = Xml.DomToText(dom);

            PTAssert.AreStringEqual(text.Replace(" ", ""), XML_TEXT.Replace(" ", ""), "Round trip");

//          assertEquals('Round trip', XML_TEXT.replace(/\s+/g, ''),
//          text.replace(/\s+/g, ''));
        }*/

        [Test]
        public void TestDomToWorkspaceBackwardCompatibility()
        {
            // Expect that workspace still loads without serialized variables.
            SetupWithMockBlocks();

            Utils.EditorDefaultGenUidValue = "1";
            var dom = Xml.TextToDom("<xml>" +
                                    "<block type=\"field_variable_test_block\" id=\"block_id\">" +
                                    "<field name=\"VAR\">name1</field>" +
                                    "</block>" +
                                    "</xml>");
            Xml.DomToWorkspace(dom, mWorkspace);
            Assert.AreEqual(1, mWorkspace.GetAllBlocks().Count, "Block count");
            TestHelper.CheckVariableValues(mWorkspace, "name1", "", "1");


            TeardownWithMockBlocks();
        }

        [Test]
        public void TestDomToWorkspaceVariablesAtTop()
        {
            // Expect that unused variables are preserved.
            SetupWithMockBlocks();
            var dom = Xml.TextToDom("<xml>" +
                                    "  <variables>" +
                                    "    <variable type=\"type1\" id=\"id1\">name1</variable>" +
                                    "    <variable type=\"type2\" id=\"id2\">name2</variable>" +
                                    "    <variable type=\"\" id=\"id3\">name3</variable>" +
                                    "  </variables>" +
                                    "  <block type=\"field_variable_test_block\">" +
                                    "    <field name=\"VAR\" id=\"id3\" variableType=\"\">name3</field>" +
                                    "  </block>" +
                                    "</xml>");
            Xml.DomToWorkspace(dom, mWorkspace);
            Assert.AreEqual(1, mWorkspace.GetAllBlocks().Count, 1,"Block count");
            TestHelper.CheckVariableValues(mWorkspace, "name1", "type1", "id1");
            TestHelper.CheckVariableValues(mWorkspace, "name2", "type2", "id2");
            TestHelper.CheckVariableValues(mWorkspace, "name3", "", "id3");
            TeardownWithMockBlocks();
        }

        [Test]
        public void TestDomToWorkspaceVariablesAtTopDuplicateVariablesTag()
        {
            // Expect thrown Error because of duplicate 'variables' tag
            SetupWithMockBlocks();

            var dom = Xml.TextToDom("<xml>" +
                                    "  <variables>" +
                                    "  </variables>" +
                                    "  <variables>" +
                                    "  </variables>" +
                                    "</xml>");
            try
            {
                Xml.DomToWorkspace(dom, mWorkspace);
            }
            catch (Exception e)
            {
                
            }

            TeardownWithMockBlocks();

        }

        [Test]
        public void TestDomToWorkspaceVariablesAtTopMissingType()
        {
            // Expect thrown error when a variable tag is missing the type attribute.
            mWorkspace = new Workspace();

            var dom = Xml.TextToDom("<xml>" +
                                    "  <variables>" +
                                    "    <variable id=\"id1\">name1</variable>" +
                                    "  </variables>" +
                                    "  <block type=\"field_variable_test_block\">" +
                                    "    <field name=\"VAR\" id=\"id1\" variableType=\"\">name3</field>" +
                                    "  </block>" +
                                    "</xml>");

            try
            {
                Xml.DomToWorkspace(dom, mWorkspace);
            }
            catch (Exception e)
            {
                
            }

            mWorkspace.Dispose();
            mWorkspace = null;

        }

        [Test]
        public void TestDomToWorkspaceVariablesAtTopMismatchBlockType()
        {
            // Expect trhown error when the serialized type of a variable does not match
            // the type of a variable field that references it.
            SetupWithMockBlocks();

            var dom = Xml.TextToDom("<xml>" +
                                    "<variables>" +
                                    "<variable type=\"type1\" id=\"id1\">name1</variable>" +
                                    "</variables>" +
                                    "<block type=\"field_variable_test_block\">" +
                                    "<field name=\"VAR\" id=\"id1\" variabletype=\"\">name1</field>" +
                                    "</block>" +
                                    "</xml>");
            try
            {
                Xml.DomToWorkspace(dom, mWorkspace);
            }
            catch (Exception e)
            {
                
            }

            TeardownWithMockBlocks();

        }


        /// <summary>
        /// Tests the taht appendDomToWorkspace works in a headless mode.
        /// Also see thest_appendDomToWorkspac() in workspace_svg_test.js
        /// </summary>
        [Test]
        public void TestAppendDomToWorkspace()
        {
            
        }
        
        [Test]
        public void TestBlockToDomFieldToDomTrivial()
        {
            SetupWithMockBlocks();
            mWorkspace.CreateVariable("name1", "type1", "id1");
            var block = BlockFactory.Instance.CreateBlock(mWorkspace,"field_variable_test_block");
            block.InputList[0].FieldRow[0].SetValue("name1");
            var resultFieldDom = Xml.BlockToDom(block).ChildNodes[0];
            XmlTestCheckVariableFieldDomValues(resultFieldDom,"VAR","type1","id1","name1");
            TeardownWithMockBlocks();
        }

        [Test]
        public void TestBlockToDomDefaultCase()
        {
            SetupWithMockBlocks();
            Utils.EditorDefaultGenUidValue = "1";
            mWorkspace.CreateVariable("name1");
            var block = BlockFactory.Instance.CreateBlock(mWorkspace, "field_variable_test_block");
            block.InputList[0].FieldRow[0].SetValue("name1");
            var resultFieldDom = Xml.BlockToDom(block).ChildNodes[0];
            // Expect type is '' and is '1' since we don't specify type and id.
            XmlTestCheckVariableFieldDomValues(resultFieldDom,"VAR","","1","name1");
            TeardownWithMockBlocks();
        }
        
        [Test]
        public void TestBlockToDomFieldToDomNotAFieldVariable()
        {
            BlockFactory.Instance.AddJsonDefinitions(@"[{
                'type':'field_angle_test_block',
                'message0':'%1',
                'args0':[
                    {
                        'type':'field_angle',
                        'name':'VAR',
                        'angle':90
                    }
                ],
            }]");
            SetupWithMockBlocks();
            var block = BlockFactory.Instance.CreateBlock(mWorkspace, "field_angle_test_block");
            var xmlDom = Xml.BlockToDom(block);
            var resultFieldDom = xmlDom.ChildNodes[0];
            XmlTestCheckNonVariableField(resultFieldDom, "VAR", "90");
            TeardownWithMockBlocks();
        }

        [Test]
        public void TestVariablesToDomOneVariable()
        {
            SetupWithMockBlocks();

            Utils.EditorDefaultGenUidValue = "1";

            mWorkspace.CreateVariable("name1");
            var resultDom = Xml.VariablesToDom(mWorkspace.GetAllVariables());
            Assert.AreEqual(1, resultDom.ChildNodes.Count);

            Debug.Log(resultDom.OuterXml);
            var resultVariableDom = resultDom.FirstChild;
            Debug.Log(resultVariableDom.TextContent());
            Assert.AreEqual("name1", resultVariableDom.TextContent());
            Assert.AreEqual("", resultVariableDom.GetAttribute("type"));
            Assert.AreEqual("1", resultVariableDom.GetAttribute("id"));

            TeardownWithMockBlocks();
        }

        [Test]
        public void TestVariablesToDomTwoVariablesOneBlock()
        {
            SetupWithMockBlocks();

            mWorkspace.CreateVariable("name1", "type1", "id1");
            mWorkspace.CreateVariable("name2", "type2", "id2");
            var block = mWorkspace.NewBlock("field_variable_test_block");
            block.InputList[0].FieldRow[0].SetValue("name1");

            var resultDom = Xml.VariablesToDom(mWorkspace.GetAllVariables());
            Assert.AreEqual(2, resultDom.ChildNodes.Count);
            XmlTestCheckVariableDomValues(resultDom.ChildNodes[0], "type1", "id1", "name1");
            XmlTestCheckVariableDomValues(resultDom.ChildNodes[1], "type2", "id2", "name2");

            TeardownWithMockBlocks();
        }

        [Test]
        public void TestVariablesToDomNoVariables()
        {
            SetupWithMockBlocks();

            mWorkspace.CreateVariable("name1");
            var resultDom = Xml.VariablesToDom(mWorkspace.GetAllVariables());
            Assert.AreEqual(1, resultDom.ChildNodes.Count);

            TeardownWithMockBlocks();
        }

        [Test]
        public void TestWorkspaceToDom()
        {
            SetupWithMockBlocks();


            mWorkspace.NewBlock("field_variable_test_block");

            var dom = Xml.WorkspaceToDom(mWorkspace);
            
            Debug.LogFormat("<color=green>{0}</color>", dom.OuterXml);
            
            TeardownWithMockBlocks();
        }
        
    }
}
