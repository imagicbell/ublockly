/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Helper functions for generating c# for blocks.
****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTGame.Blockly
{
    public partial class CSharpGenerator : Generator
    {
        public override CodeName Name
        {
            get { return CodeName.CSharp; }
        }

        public CSharpGenerator(Names variableNames) : base(variableNames)
        {
        }

        protected override void Init(Workspace workspace)
        {
            mFuncMap.Clear();
            mVariableNames.Reset();

            List<VariableModel> variables = workspace.GetAllVariables();
            if (variables.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (VariableModel variable in variables)
                {
                    string type = variable.Type;
                    if (string.IsNullOrEmpty(type))
                    {
                        type = "var";
                    }
                    else
                    {
                        foreach (string[] strs in DataTypes.DB.Values)
                        {
                            if (!string.IsNullOrEmpty(strs.FirstOrDefault(s => s.ToLower().Equals(type.ToLower()))))
                                type = strs[0];
                        }
                    }
                    sb.Append(type + " " + mVariableNames.GetName(variable.Name, Blockly.VARIABLE_CATEGORY_NAME) + ";\n");
                }
                mFuncMap["variables"] = new KeyValuePair<string, string>(null, sb.ToString());
            }
        }

        protected override string Finish(string code)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in mFuncMap.Values)
                sb.Append(pair.Value + "\n\n");

            // Clean up temporary data.
            mFuncMap.Clear();
            mVariableNames.Reset();

            return sb + code;
        }

        protected override string Scrub(Block block, string code)
        {
            //todo: add comment
            return base.Scrub(block, code);
        }

        protected override string ScrubNakedValue(string code)
        {
            return "var _ = " + code + ";\n";
        }

        /// <summary>
        /// Encode a string as a properly escaped C# string, complete with quotes.
        /// </summary>
        public string Quote(string text)
        {
            string newtext = text.Replace("\\", "\\\\").Replace("\n", "\\\n").Replace("'", "\\\'");
            return "\"" + newtext + "\"";
        }
    }
}