/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Xml;

namespace UBlockly
{
    public class MathIsDivisibleByMutator : Mutator
    {
        private const string DIVISOR_INPUT = "DIVISOR";
        private const string DIVISIBLE_BY = "DIVISIBLE_BY";
        private static List<string> CHECK = new List<string> {"Number"}; 
        
        private FieldDropdown mDropdown;
        private MemorySafeMutatorObserver mObserver;
        
        public override bool NeedEditor
        {
            get { return false; }
        }
        
        protected override void MutateInternal()
        {
            if (mBlock != null)
                UpdateInternal(mDropdown.GetValue().Equals(DIVISIBLE_BY));
        }

        protected override void OnAttached()
        {
            mDropdown = mBlock.GetField("PROPERTY") as FieldDropdown;
            if (mDropdown == null)
                throw new Exception("FieldDropDown \"PROPERTY\" not found.");

            UpdateInternal(mDropdown.GetValue().Equals(DIVISIBLE_BY));
            
            //register observer
            mObserver = new MemorySafeMutatorObserver(this);
            mDropdown.AddObserver(mObserver);
        }

        protected override void OnDetached()
        {
            //remove observer
            mDropdown.RemoveObserver(mObserver);
        }

        public override XmlElement ToXml()
        {
            XmlElement xmlElement = XmlUtil.CreateDom("mutation");
            bool divisorInput = mDropdown.GetValue().Equals(DIVISIBLE_BY);
            xmlElement.SetAttribute("divisor_input", divisorInput ? "true" : "false");
            return xmlElement;
        }

        public override void FromXml(XmlElement xmlElement)
        {
            bool divisorInput = xmlElement.GetAttribute("divisor_input") == "true";
            UpdateInternal(divisorInput);
        }

        protected void UpdateInternal(bool divisible)
        {
            Input divisorInput = mBlock.GetInput(DIVISOR_INPUT);
            if (divisible && divisorInput == null)
            {
                divisorInput = InputFactory.Create(Blockly.INPUT_VALUE, DIVISOR_INPUT, Blockly.ALIGN_LEFT, CHECK);
                mBlock.AppendInput(divisorInput);
            }
            else if (!divisible && divisorInput != null)
            {
                mBlock.RemoveInput(divisorInput);
            }
        }
    }
}
