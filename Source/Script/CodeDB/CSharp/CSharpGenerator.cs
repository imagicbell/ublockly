/****************************************************************************

Helper functions for generating c# for blocks.

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


using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UBlockly
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
                        foreach (string[] strs in Define.DataTypeDB.Values)
                        {
                            if (!string.IsNullOrEmpty(strs.FirstOrDefault(s => s.ToLower().Equals(type.ToLower()))))
                                type = strs[0];
                        }
                    }
                    sb.Append(type + " " + mVariableNames.GetName(variable.Name, Define.VARIABLE_CATEGORY_NAME) + ";\n");
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
