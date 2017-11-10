/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Helper functions for generating lua for blocks.
****************************************************************************/

using System.Text;

namespace UBlockly
{
    public partial class LuaGenerator : Generator
    {
        public override CodeName Name
        {
            get { return CodeName.Lua; }
        }

        public LuaGenerator(Names variableNames) : base(variableNames)
        {
        }

        protected override void Init(Workspace workspace)
        {
            mFuncMap.Clear();
            mVariableNames.Reset();
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
            return "local _ = " + code + "\n";
        }

        /// <summary>
        /// Encode a string as a properly escaped Lua string, complete with quotes.
        /// </summary>
        public string Quote(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "\'\'";
            
            string newtext = text.Replace("\\", "\\\\").Replace("\n", "\\\n").Replace("'", "\\\'");
            return "\'" + newtext + "\'";
        }
    }
}
