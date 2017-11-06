/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Collections for some definitions for code generations and interpreting
****************************************************************************/

using System;
using System.ComponentModel;

namespace PTGame.Blockly
{
    public enum CodeName
    {
        CSharp,
        Lua,
    }

    public enum ControlFlowType
    {
        None,
        Break,
        Continue,
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class CodeGeneratorAttribute : Attribute
    {
        [Description("method for generating block code string")]
        public CodeGeneratorAttribute() {}
        public string BlockType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CodeInterpreterAttribute : Attribute
    {
        [Description("method for interpreting to implement block code")]
        public CodeInterpreterAttribute() {}
        public string BlockType { get; set; }
    }
}