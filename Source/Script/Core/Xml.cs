/****************************************************************************

Copyright 2016 liangxiegame@163.com
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
using System.Xml;
using UnityEngine;

namespace UBlockly
{
    public class Xml
    {

        /// <summary>
        /// Encode a block tree as XML.
        /// </summary>
        /// <param name="workspace"> The workspace containing blocks.</param>
        /// <param name="optNoId"> Ture if the encoder should skip the block ids.</param>
        /// <returns> XML document.</returns>
        public static XmlNode WorkspaceToDom(Workspace workspace, bool optNoId = false)
        {
            var xml = XmlUtil.CreateDom("xml");
            xml.AppendChild(VariablesToDom(workspace.GetAllVariables()));
            var blocks = workspace.GetTopBlocks(true);
            foreach (var block in blocks)
            {
                xml.AppendChild(BlockToDomWithXY(block, optNoId));
            }
            return xml;
        } 
        
        /// <summary>
        /// Converts plain text into a DOM structure
        /// Throws an error if XML doesn't parse.
        /// </summary>
        /// <param name="text"> Text representation.</param>
        public static XmlNode TextToDom(string text)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(text);
            // The DOM should have one and only one top-level node, an XML tag.
            if (null == dom || dom.FirstChild == null ||
                !string.Equals(dom.FirstChild.Name.ToLower(), "xml") ||
                dom.FirstChild != dom.LastChild)
            {
                // Whatever we got back from the parser is not XML.
                throw new Exception("Blockly.Xml.textToDom did not obtain a valid XML tree");
            }
            return dom.FirstChild;
        }

        /// <summary>
        /// Converts a DOM structure into plain text.
        /// </summary>
        /// <param name="dom"> A tree of XML elements.</param>
        /// <returns>Text representation.</returns>
        public static string DomToText(XmlNode dom)
        {
            using (var sw = new System.IO.StringWriter())
            {
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    xw.Indentation = 4;
                    dom.WriteTo(xw);
                }
                return sw.ToString();
            }
        }

        /// <summary>
        /// Decode an XML DOM and create blocks on the workspace.
        /// </summary>
        /// <param name="dom">XML DOM</param>
        /// <param name="workspace">The WOrkspace</param>
        public static List<string> DomToWorkspace(XmlNode xml, Workspace workspace)
        {
            List<string> newBlockIds = new List<string>(); // A list of block ids added by this call.
            var childCount = xml.ChildNodes.Count;
            int width = 0; // Not used in LTR.
            var variablesFirst = true;
            for (var i = 0; i < childCount; i++)
            {
                var xmlChild = xml.ChildNodes[i];

                var name = xmlChild.Name.ToLower();
                if (string.Equals(name, "block") ||
                    string.Equals(name, "shadow"))
                {
                    // Allow top-level shadow blocks if recordUndo is disabled since
                    // that meas an undo is in progress. Such a block is expected
                    // to be moved to a nested destination in the next operation.
                    var block = Xml.DomToBlock(xmlChild, workspace);
                    newBlockIds.Add(block.ID);
                    int blockX = 10;
                    int blockY = 10;
                    bool blockXParseSucceed = int.TryParse(xmlChild.GetAttribute("x"), out blockX);
                    bool blockYParseSucceed = int.TryParse(xmlChild.GetAttribute("y"), out blockY);

                    if (blockXParseSucceed && blockYParseSucceed)
                    {
                        block.XY = new Vector2(workspace.RTL ? width - blockX : blockX, blockY);
                    }
                    variablesFirst = false;
                }
                else if (string.Equals(name, "shadow"))
                {
                    variablesFirst = false;
                }
                else if (string.Equals(name, "variables"))
                {
                    if (variablesFirst)
                    {
                        Xml.DomToVariables(xmlChild, workspace);
                    }
                    else
                    {
                        throw new Exception("\'variables\' tag must exist once before block and " +
                                            "shadow tag elements in the workspace XML, but it was found in " +
                                            "another location.");
                    }
                    variablesFirst = false;
                }
            }
            workspace.UpdateVariableStore(false);
            return newBlockIds;
        }

        /// <summary>
        /// Decode an XML DOM and create blocks on the workspace. Position the new 
        /// blocks immediately below prior blocks,aligned by theri starting edge.
        /// </summary>
        /// <param name="xml"> The XML DOM.</param>
        /// <param name="workspace"> The workspace to add to.</param>
        /// <returns> An array containing new block ids.</returns>
        public static List<string> AppendDomToWorkspace(XmlNode xml, Workspace workspace)
        {
            // load the new blocks into theworkspace and get the ids of the ne blocks
            var newBlockIds = Xml.DomToWorkspace(xml, workspace);
            return newBlockIds;
        }

        /// <summary>
        /// Encode a list of variables as XML.
        /// </summary>
        /// <param name="variableList"> List of all variable</param>
        /// <returns> List of XML elements</returns>
        public static XmlNode VariablesToDom(List<VariableModel> variableList)
        {
            var variables = XmlUtil.CreateDom("variables");
            foreach (var variable in variableList)
            {
                var element = XmlUtil.CreateDom("variable", null, variable.Name);
                element.SetAttribute("type", variable.Type);
                element.SetAttribute("id", variable.ID);
                variables.AppendChild(element);
            }
            return variables;
        }

        /// <summary>
        /// Encode a block subtree as XML with XY coordinates.
        /// </summary>
        /// <param name="block"> The root block to encode.</param>
        /// <param name="optNoId"> True if the encoder should skip the block id.</param>
        /// <returns> Tree of xml elements</returns>
        public static XmlNode BlockToDomWithXY(Block block, bool optNoId)
        {
            int width = 0; // Not used in LTR.
            var element = Xml.BlockToDom(block, optNoId);
            var xy = block.XY;
            element.SetAttribute("x", Mathf.Round(block.Workspace.RTL ? width - xy.x : xy.x).ToString());
            element.SetAttribute("y", Mathf.Round(xy.y).ToString());
            return element;
        }
        

        /// <summary>
        /// Encode a block subtree as XML.
        /// </summary>
        /// <param name="block">The root block to encode.</param>
        /// <param name="optNoId">True if the encoder should skip the block id.</param>
        /// <returns></returns>
        public static XmlNode BlockToDom(Block block, bool optNoId = false)
        {
            var element = XmlUtil.CreateDom(block.IsShadow ? "shadow" : "block");
            element.SetAttribute("type", block.Type);
            if (!optNoId)
            {
                element.SetAttribute("id", block.ID);
            }

            if (block.Mutator != null)
            {
                // Custom data for an advanced block.
                var container = block.Mutator.ToXml();
                if (container != null)
                    element.AppendChild(container);
            }

            Action<Field> fieldToDom = delegate(Field field)
            {
//                if  TODO: editable is true (!string.IsNullOrEmpty(field.Name) && field.Editable)
                if (!string.IsNullOrEmpty(field.Name))
                {
                    var container = XmlUtil.CreateDom("field", null, field.GetValue());
                    container.SetAttribute("name", field.Name);
                    if (field is FieldVariable)
                    {
                        var variable = block.Workspace.GetVariable(field.GetValue());
                        if (null != variable)
                        {
                            container.SetAttribute("id", variable.ID);
                            container.SetAttribute("variableType", variable.Type);

                        }
                    }
                    element.AppendChild(container);
                }
            };

            foreach (var input in block.InputList)
            {
                foreach (var field in input.FieldRow)
                {
                    fieldToDom(field);
                }
            }

            if (!string.IsNullOrEmpty(block.Data))
            {
                var dataElement = XmlUtil.CreateDom("data", null, block.Data);
                element.AppendChild(dataElement);
            }

            foreach (var input in block.InputList)
            {
                XmlNode container = null;
                var empty = true;
                if (input.Type == Define.EConnection.DummyInput)
                {
                    continue;
                }
                else
                {
                    var childBlock = input.Connection.TargetBlock;
                    if (input.Type == Define.EConnection.InputValue)
                    {
                        container = XmlUtil.CreateDom("value");
                    }
                    else if (input.Type == Define.EConnection.NextStatement)
                    {
                        container = XmlUtil.CreateDom("statement");
                    }

                    var shadow = input.Connection.ShadowDom;

                    if (null != shadow && (null == childBlock || !childBlock.IsShadow))
                    {
                //        container.appendChild(Blockly.Xml.cloneShadow_(shadow));
                    }
                    if (null != childBlock)
                    {
                        container.AppendChild(Xml.BlockToDom(childBlock, optNoId));
                        empty = false;
                    }
                }
                container.SetAttribute("name",input.Name);
                
                if (!empty)
                {
                    element.AppendChild(container);
                }
            }
            
            if (block.Collapsed)
            {
                element.SetAttribute("collapsed", "true");
            }
            if (block.Disabled)
            {
                element.SetAttribute("disabled", "true");
            }
            if (!block.Deletable && !block.IsShadow)
            {
                element.SetAttribute("deletable", "false");
            }
            if (!block.Movable && !block.IsShadow)
            {
                element.SetAttribute("movable", "false");
            }
            if (!block.Editable)
            {
                element.SetAttribute("editable", "false");
            }

            var nextBlock = block.NextBlock;
            if (null != nextBlock)
            {
                var container = XmlUtil.CreateDom("next", null, BlockToDom(nextBlock, optNoId));
                element.AppendChild(container);
            }
           
            return element;
        }
        
        /// <summary>
        /// Decode an XML list of variables and add the variables to the worksapce.
        /// </summary>
        /// <param name="xmlVariables"> xmlVariables List of XML vale elements.riab</param>
        /// <param name="workspace"> The worksapce to which the variable should be added</param>
        public static void DomToVariables(XmlNode xmlVariables, Workspace workspace)
        {
            for (int i = 0; i < xmlVariables.ChildNodes.Count; i++)
            {
                XmlNode xmlChild = xmlVariables.ChildNodes[i];
                var type = xmlChild.GetAttribute("type");
                var id = xmlChild.GetAttribute("id");
                var name = xmlChild.InnerXml;

                if (type == null)
                {
                    throw new Exception("Variable with id, " + id + " is without a type");
                }
                workspace.CreateVariable(name, type, id);
            }
        }


        /// <summary>
        /// Decode an XML block tag and create a block (and possibly sub blocks) on the workspace.
        /// </summary>
        /// <param name="xmlBlock"> XML block element.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>The root block created.</returns>
        public static Block DomToBlock(XmlNode xmlBlock, Workspace workspace)
        {
            var topBlock = DomToBlockHeadless(xmlBlock, workspace);
            return topBlock;
        }

        /// <summary>
        /// Decode an XML block tag and crete a block (and possbly sub blocks) on the
        /// workspace.
        /// </summary>
        /// <param name="xmlBlock"> XML block element.</param>
        /// <param name="workspace"> The workspace</param>
        public static Block DomToBlockHeadless(XmlNode xmlBlock, Workspace workspace)
        {
            var prototypeName = xmlBlock.GetAttribute("type");
            var id = xmlBlock.GetAttribute("id");
            Block block = workspace.NewBlock(prototypeName, id);
            Block blockChild = null;

            for (var i = 0; i < xmlBlock.ChildNodes.Count; i++)
            {
                var xmlChild = xmlBlock.ChildNodes[i];
                if (xmlChild.NodeType == (XmlNodeType) 3)
                {
                    // Ignore any text at the <block>level. It's all whitespace anyway.
                    continue;
                }
                Input input = null;
                // Find any enclosed blocks or shadows in this tag.
                XmlNode childBlockNode = null;
                XmlNode childShadowNode = null;
                for (var j = 0; j < xmlChild.ChildNodes.Count; j++)
                {
                    var grandchildNode = xmlChild.ChildNodes[j];
                    if (grandchildNode.NodeType == (XmlNodeType) 1)
                    {
                        if (string.Equals(grandchildNode.NodeName().ToLower(), "block"))
                        {
                            childBlockNode = grandchildNode;
                        }
                        else if (string.Equals(grandchildNode.NodeName().ToLower(), "shadow"))
                        {
                            childShadowNode = grandchildNode;
                        }
                    }
                }
                // Use the shadow block if there is no child block.
                if (null == childBlockNode && null != childShadowNode)
                {
                    childBlockNode = childShadowNode;
                }

                var name = xmlChild.GetAttribute("name");
                switch (xmlChild.NodeName().ToLower())
                {
                    case "mutation":
                        // Custom data for an advanced block.
                        if (block.Mutator != null)
                            block.Mutator.FromXml((XmlElement)xmlChild);
                        break;
                    case "data":
                        block.Data = xmlChild.TextContent();
                        break;

                    case "title":
                    case "field":
                    {
                        var field = block.GetField(name);
                        var text = xmlChild.TextContent();
                        if (field is FieldVariable)
                        {
                            // TODO (marisaleung): When we change setValue and getValue to
                            // interact with id's instead of names,update this so that we get
                            // the variable based on id instead of textContent.
                            var type = string.IsNullOrEmpty(xmlChild.GetAttribute("variableType"))
                                ? xmlChild.GetAttribute("variableType")
                                : "";
                            var variable = workspace.GetVariable(text);
                            if (null == variable)
                            {
                                variable = workspace.CreateVariable(text, type, xmlChild.GetAttribute(id));
                            }

                            if (!string.IsNullOrEmpty(type))
                            {
                                if (!string.Equals(variable.Type, type))
                                {
                                    throw new Exception("Serialized variable type with id \'" +
                                                        variable.ID + "\' had type " + variable.Type + ", and " +
                                                        "does not match variable field that references it: " +
                                                        Xml.DomToText(xmlChild) + ".");
                                }
                            }
                        }

                        if (null == field)
                        {
                            Console.WriteLine("Ignoring non-existent field " + name + " in block " + prototypeName);
                            break;
                        }
                        field.SetValue(text);
                    }
                        break;
                    case "value":
                    case "statement":
                        input = block.GetInput(name);
                        if (null == input)
                        {
                            Console.WriteLine("Ignoring non-existent input " + name + " in block " +
                                              prototypeName);
                            break;
                        }
                        if (null != childShadowNode)
                        {
//                            input.connection.setShadowDom(childShadowNode);
                        }
                        if (null != childBlockNode)
                        {
                            blockChild = Xml.DomToBlockHeadless(childBlockNode, workspace);
                            if (null != blockChild.OutputConnection)
                            {
                                input.Connection.Connect(blockChild.OutputConnection);
                            }
                            else if (null != blockChild.PreviousConnection)
                            {
                                input.Connection.Connect(blockChild.PreviousConnection);
                            }
                            else
                            {
                                throw new Exception("Child block does not have output or previous statement.");
                            }
                        }
                        break;
                    case "next":
                        if (null != childShadowNode && null != block.NextConnection)
                        {
//                            block.nextConnection.setShadowDom(childShadowNode);
                        }
                        if (null != childBlockNode)
                        {
                            blockChild = Xml.DomToBlockHeadless(childBlockNode, workspace);
                            block.NextConnection.Connect(blockChild.PreviousConnection);
                        }
                        break;
                    default:
                        // Unknown tag; ignore. Same principle as HTML parsers.
                        Console.WriteLine("Ignoring unknown tag: " + xmlChild.NodeName());
                        break;
                }
            }

            var inline = xmlBlock.GetAttribute("inline");
            if (!string.IsNullOrEmpty(inline))
            {
                block.SetInputsInline(string.Equals(inline, "true"));
            }
            var disabled = xmlBlock.GetAttribute("disabled");
            if (!string.IsNullOrEmpty(disabled))
            {
                block.Disabled = string.Equals(disabled, "true");
            }

            var deletable = xmlBlock.GetAttribute("deletable");
            if (!string.IsNullOrEmpty(deletable))
            {
                block.Deletable = string.Equals(deletable, "true");
            }

            var movable = xmlBlock.GetAttribute("movable");
            if (!string.IsNullOrEmpty(movable))
            {
                block.Movable = string.Equals(movable, "true");
            }

            var editable = xmlBlock.GetAttribute("editable");
            if (!string.IsNullOrEmpty(editable))
            {
                block.Editable = string.Equals(editable, "true");
            }

            var collapsed = xmlBlock.GetAttribute("collapsed");
            if (!string.IsNullOrEmpty(collapsed))
            {
                block.Collapsed = string.Equals(collapsed, "true");
            }
            return block;
        }


        /// <summary>
        /// Remove any "next" block (statements in a stack).
        /// </summary>
        /// <param name="xmlBlock"> XML block element.</param>
        public void DeleteNext(XmlNode xmlBlock)
        {
            for (int i = 0; i < xmlBlock.ChildNodes.Count; i++)
            {
                var child = xmlBlock.ChildNodes[i];
                if (string.Equals(child.NodeName().ToLower(), "next"))
                {
                    xmlBlock.RemoveChild(child);
                    break;
                }
            }
        }
    }

    public static class XmlUtil
    {
        static XmlDocument mDocument = new XmlDocument();
        static List<XmlNode> GetElementsByTagName(XmlNode node, string tagName, List<XmlNode> nodeList)
        {
            if (null == nodeList) nodeList = new List<XmlNode>();

            if (null != node.ChildNodes && node.ChildNodes.Count != 0)
            {
                var childNodes = node.ChildNodes;
                foreach (XmlNode childNode in childNodes)
                {
                    if (string.Equals(childNode.Name, tagName))
                    {
                        nodeList.Add(childNode);
                    }

                    GetElementsByTagName(childNode, tagName, nodeList);
                }
            }

            return nodeList;
        }

        public static List<XmlNode> GetElementsByTagName(this XmlNode self, string tagName)
        {
            return GetElementsByTagName(self, tagName, null);
        }

        public static string NodeName(this XmlNode self)
        {
            return self.Name;
        }

        public static string GetAttribute(this XmlNode self, string attribName)
        {
            return self.Attributes[attribName] == null ? null : self.Attributes[attribName].Value;
        }

        public static void SetAttribute(this XmlNode self, string attribName, string value)
        {

            if (null == self.GetAttribute(attribName))
            {
                var attribute = self.OwnerDocument.CreateAttribute(attribName);
                self.Attributes.Append(attribute);
            }
            self.Attributes[attribName].Value = value;
        }

        public static string TextContent(this XmlNode self)
        {
            return self.InnerText;
        }

        public static XmlElement CreateDom(string name, string attributes = null, string value =  null)
        {
            var retDom = mDocument.CreateElement(name);
            if (string.IsNullOrEmpty(value))
            {

            }
            else
            {
                retDom.InnerText = value;
            }
            return retDom;
        }
        
        public static XmlElement CreateDom(string name, string attributes, XmlNode value)
        {
            var retDom = mDocument.CreateElement(name);
            retDom.AppendChild(value);
            return retDom;
        }
    }
}
