/****************************************************************************

Utility functions for generating executable code from Blockly code.

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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace UBlockly
{
    public struct CodeStruct
    {
        public string code;
        public int order;

        public CodeStruct(string code)
        {
            this.code = code;
            this.order = -1;
        }

        public CodeStruct(string code, int order)
        {
            this.code = code;
            this.order = order;
        }

        public static CodeStruct Empty
        {
            get { return new CodeStruct("", -1); }
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(code); }
        }
    }
    
    public abstract class Generator
    {
        public abstract CodeName Name { get; }

        /// <summary>
        /// methods for generating code
        /// </summary>
        protected Dictionary<string, MethodInfo> mCodeMap;
        
        /// <summary>
        /// handle variable names
        /// </summary>
        protected Names mVariableNames;

        protected Generator(Names variableNames)
        {
            InitCodeDB();
            mVariableNames = variableNames;
        }

        /// <summary>
        /// collect all code generation methods
        /// </summary>
        protected void InitCodeDB()
        {
            MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.IsDefined(typeof(CodeGeneratorAttribute), false))
                {
                    if (mCodeMap == null) mCodeMap = new Dictionary<string, MethodInfo>();
                    mCodeMap[((CodeGeneratorAttribute) method.GetCustomAttributes(typeof(CodeGeneratorAttribute), false)[0]).BlockType] = method;
                }
            }
        }
        
        #region Code Generation

        /// <summary>
        /// Generate code for all blocks in the workspace to the specified language.
        /// </summary>
        public string WorkspaceToCode(Workspace workspace)
        {
            Init(workspace);

            StringBuilder codeSB = new StringBuilder(); 
            var blocks = workspace.GetTopBlocks(true);
            foreach (var block in blocks)
            {
                var line = this.BlockToCode(block).code;
                if (!string.IsNullOrEmpty(line))
                {
                    if (block.OutputConnection != null)
                    {
                        line = this.ScrubNakedValue(line);
                    }
                    codeSB.Append(line + "\n");
                }
            }

            string code = this.Finish(codeSB.ToString());
           
            // Final scrubbing of whitespace.
            Regex rgx = new Regex(@"^\s+\n");
            code = rgx.Replace(code, "");
            rgx = new Regex(@"\n\s+$");
            code = rgx.Replace(code, "\n");
            rgx = new Regex(@"[ \t]+\n");
            code = rgx.Replace(code, "\n");
            
            return code;
        }
        
        /// <summary>
        /// Generate code for the specified block (and attached blocks).
        /// </summary>
        public CodeStruct BlockToCode(Block block)
        {
            if (block == null) return CodeStruct.Empty;
            
            if (block.Disabled)
                return this.BlockToCode(block.NextBlock);

            MethodInfo func;
            if (!mCodeMap.TryGetValue(block.Type, out func))
            {
                throw new Exception(string.Format("Language {0} does not know how to generate code for block type {1}.", Name, block.Type));
            }

            var code = func.Invoke(this, new object[] {block});
            if (code is CodeStruct)
            {
                CodeStruct codeStruct = (CodeStruct) code;
                if (codeStruct.order >= 0 && block.OutputConnection == null)
                    throw new Exception(string.Format("Expecting string from statement block {0}.", block.Type));
                return new CodeStruct(this.Scrub(block, codeStruct.code), codeStruct.order);
            }
            if (code is string)
            {
                return new CodeStruct(this.Scrub(block, (string) code));
            }
            if (code == null)
            {
                return CodeStruct.Empty;
            }
            
            throw new Exception(string.Format("Invalid code generated for block: {0}", block.Type));
        }

        /// <summary>
        /// Generate code representing the specified value input.
        /// </summary>
        /// <param name="name">The name of the input.</param>
        /// <param name="outerOrder">The maximum binding strength (minimum order value) of any operators adjacent to "block".</param>
        public string ValueToCode(Block block, string name, int outerOrder)
        {
            if (outerOrder < 0)
                throw new Exception(string.Format("Expecting valid order from block {0}", block.Type));

            var targetBlock = block.GetInputTargetBlock(name);
            if (targetBlock == null)
                return "";

            var tuple = this.BlockToCode(targetBlock);
            if (tuple.IsEmpty)
                return "";//disabled block

            if (tuple.order < 0)
                throw new Exception(string.Format("Expecting valid order from value block: {0}", targetBlock.Type));

            bool parensNeeded = false;
            int innerOrder = tuple.order;
            if (outerOrder == innerOrder && (outerOrder == 0 || innerOrder == 99))
            {
                // Don't generate parens around NONE-NONE and ATOMIC-ATOMIC pairs.
                parensNeeded = false;
            }
            else if (outerOrder <= innerOrder)
            {
                // The operators outside this code are stronger than the operators
                // inside this code.  To prevent the code from being pulled apart,
                // wrap the code in parentheses.
                parensNeeded = true;
                //check for special exceptions
                if (mOrderExceptions != null && mOrderExceptions.Length > 0)
                {
                    for (int i = 0; i < mOrderExceptions.Length; i++)
                    {
                        if (mOrderExceptions[i].Key == outerOrder && mOrderExceptions[i].Value == innerOrder)
                        {
                            parensNeeded = false;
                            break;
                        }
                    }
                }
            }
            if (parensNeeded)
                return "(" + tuple.code + ")";
            return tuple.code;
        }

        /// <summary>
        /// Generate code representing the specified value input, WITH defaultCode
        /// </summary>
        public string ValueToCode(Block block, string name, int outerOrder, string defaultCode)
        {
            string code = this.ValueToCode(block, name, outerOrder);
            if (string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(defaultCode))
                code = defaultCode;
            return code;
        }

        /// <summary>
        /// Generate code representing the statement. Indent the code.
        /// </summary>
        public string StatementToCode(Block block, string name)
        {
            var targetBlock = block.GetInputTargetBlock(name);
            if (targetBlock == null)
                return "";
            
            var tuple = this.BlockToCode(targetBlock);
            if (tuple.IsEmpty)
                return "";//disabled block
            
            if (tuple.order >= 0)
                throw new Exception(string.Format("Expecting code from statement block: {0}", targetBlock.Type));

            return this.PrefixLines(tuple.code, Indent);
        }

        /// <summary>
        /// Generate code representing the statement. Indent the code. WITH defaultCode
        /// </summary>
        public string StatementToCode(Block block, string name, string defaultCode)
        {
            string code = this.StatementToCode(block, name);
            if (string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(defaultCode))
                code = defaultCode;
            return code;
        }

        #endregion

        public const string FUNCTION_NAME_PLACEHOLDER = "{leCUI8hutHZI4480Dc}";
        protected Dictionary<string, KeyValuePair<string, string>> mFuncMap = new Dictionary<string, KeyValuePair<string, string>>();

        /// <summary>
        /// Define a function to be included in the generated code.
        /// The first time this is called with a given desiredName, 
        /// the code is saved and an actual name is generated.  
        /// Subsequent calls with the same desiredName have no effect but have the same return value.
        /// It is up to the caller to make sure the same desiredName is not used for different code values.
        /// The code gets output when Blockly.Generator.finish() is called.
        /// </summary>
        /// <param name="desiredName"></param>
        /// <param name="code"></param>
        /// <returns>The actual name of the new function.  This may differ from desiredName if the former has already been taken by the user.</returns>
        public string ProvideFunction(string desiredName, string code)
        {
            KeyValuePair<string, string> funcPair;
            if (mFuncMap.TryGetValue(desiredName, out funcPair))
                return funcPair.Key;

            string funcName = mVariableNames.GetDistinctName(desiredName);
            string codeText = code.Replace(FUNCTION_NAME_PLACEHOLDER, funcName);
            // Change all '  ' indents into the desired indent.
            // To avoid an infinite loop of replacements, change all indents to '\0'
            // character first, then replace them all with the indent.
            // We are assuming that no provided functions contain a literal null char.
            string oldCodeText = null;
            while (!codeText.Equals(oldCodeText))
            {
                oldCodeText = codeText;
                Regex regex = new Regex(@"^((  )*)  ");
                codeText = regex.Replace(codeText, "$1\0");
            }
            codeText = codeText.Replace("\0", this.Indent);
            mFuncMap[desiredName] = new KeyValuePair<string, string>(funcName, codeText);
            return funcName;
        }
        
        /// <summary>
        /// Add the procedure definition block code to mFuncMap
        /// </summary>
        /// <param name="desiredName"></param>
        /// <param name="code"></param>
        public void AddFunction(string desiredName, string code)
        {
            mFuncMap[desiredName] = new KeyValuePair<string, string>(desiredName, code);
        }

        #region Properties

        /// <summary>
        /// List of outer-inner pairings that do NOT require parentheses.
        /// </summary>
        protected KeyValuePair<int, int>[] mOrderExceptions;

        public string Indent = "    ";

        #endregion
        
        
        #region Methods for Overrides
        
        /// <summary>
        /// Hook for code to run before code generation starts.
        /// Subclasses may override this, e.g. to initialise the database of variable names.
        /// </summary>
        protected virtual void Init(Workspace workspace)
        {
        }

        /// <summary>
        /// Hook for code to run at end of code generation.
        /// Subclasses may override this, e.g. to prepend the generated code with the variable definitions.
        /// </summary>
        protected virtual string Finish(string code)
        {
            return code;
        }
        
        /// <summary>
        /// Common tasks for generating code from blocks.  
        /// This is called from blockToCode and is called on every block, not just top level blocks.
        /// Subclasses may override this, e.g. to generate code for statements following
        /// the block, or to handle comments for the specified block and any connected value blocks.
        /// </summary>
        protected virtual string Scrub(Block block, string code)
        {
            string nextCode = "";
            if (block.NextBlock != null)
                nextCode = this.BlockToCode(block.NextBlock).code;
            return code + nextCode;
        }

        /// <summary>
        /// Naked values are top-level blocks with outputs that aren't plugged into anything.
        /// Subclasses may override this, e.g. if their language does not allow naked values.
        /// </summary>
        protected virtual string ScrubNakedValue(string code)
        {
            return code;
        }
        
        #endregion

        /// <summary>
        /// Prepend a common prefix onto each line of code.
        /// </summary>
        public string PrefixLines(string text, string prefix)
        {
            Regex rgx = new Regex(@"(?!\n$)\n");
            return prefix + rgx.Replace(text, '\n' + prefix);
        }

        /// <summary>
        /// Add an infinite loop trap to the contents of a loop.
        /// If loop is empty, add a statment prefix for the loop block.
        /// </summary>
        public string AddLoopTrap(string branch, string id)
        {
            return branch;
        }
    }
}
