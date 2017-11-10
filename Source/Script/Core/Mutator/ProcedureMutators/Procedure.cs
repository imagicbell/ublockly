/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
****************************************************************************/
using System.Collections.Generic;
using System.Xml;

namespace UBlockly
{
    /// <summary>
    /// Describes a procedure for procedure mutators.
    /// </summary>
    public class Procedure
    {
        private string mName;
        public string Name { get { return mName; } }

        private List<string> mArguments;
        public List<string> Arguments { get { return mArguments; } }
        
        private bool mDefinitionHasStatementBody;
        public bool DefinitionHasStatementBody { get { return mDefinitionHasStatementBody; } }

        /// <summary>
        /// Constructs a new Procedure with the given arguments.
        /// </summary>
        /// <param name="name">The name of the procedure, or null if not yet defined.</param>
        /// <param name="argumentNames">The list of parameter names, possibly empty.</param>
        /// <param name="definitionHasStatements">Whether the procedure definition includes</param>
        public Procedure(string name, List<string> argumentNames, bool definitionHasStatements)
        {
            mName = name;
            mArguments = argumentNames;
            mDefinitionHasStatementBody = definitionHasStatements;
        }

        /// <summary>
        /// Constructs a new Procedure with the same parameters, but a new name.
        /// </summary>
        /// <param name="newProcedureName">The name to use on the constructed Procedure.</param>
        /// <returns></returns>
        public Procedure CloneWithName(string newProcedureName)
        {
            return new Procedure(newProcedureName, mArguments, mDefinitionHasStatementBody);
        }

        public static XmlElement Serialize(Procedure info, bool isDefinition)
        {
            XmlElement xmlElement = XmlUtil.CreateDom("mutation");
            if (isDefinition)
            {
                if (!info.DefinitionHasStatementBody)
                {
                    xmlElement.SetAttribute("statements", "false");
                }
            }
            else
            {
                string procName = info.Name;
                xmlElement.SetAttribute("name", procName);
            }
            
            foreach (string argument in info.Arguments)
            {
                XmlElement parameter = XmlUtil.CreateDom("arg");
                parameter.SetAttribute("name", argument);
                xmlElement.AppendChild(parameter);
            }
            
            return xmlElement;
        }

        public static Procedure Deserialize(XmlElement xmlElement)
        {
            string name = null;
            if (xmlElement.HasAttribute("name"))
                name = xmlElement.GetAttribute("name");

            bool hasStatement = true;
            if (xmlElement.HasAttribute("statements"))
            {
                hasStatement = xmlElement.GetAttribute("statements") != "false";
            }

            List<string> arguments = new List<string>();
            foreach (XmlElement childNode in xmlElement.ChildNodes)
            {
                if (childNode.Name.Equals("arg"))
                    arguments.Add(childNode.GetAttribute("name"));
            }
            return new Procedure(name, arguments, hasStatement);
        }
    }
    
    /// <summary>
    /// Describes the re-indexing of argument order during a procedure mutation. Used to ensure
    /// values of arguments that are present both before and after are reconnected to the right input.
    /// </summary>
    public struct ProcedureArgumentIndexUpdate
    {
        public int Before;
        public int After;

        /// <summary>
        /// Constructor for a new ArgumentIndexUpdate.
        /// </summary>
        /// <param name="before">The argument's index before the change.</param>
        /// <param name="after">The argument's index after the change.</param>
        public ProcedureArgumentIndexUpdate(int before, int after)
        {
            this.Before = before;
            this.After = after;
        }
    }
}
