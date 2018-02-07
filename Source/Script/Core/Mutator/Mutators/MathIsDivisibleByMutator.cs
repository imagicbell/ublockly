/****************************************************************************

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

namespace UBlockly
{
    [MutatorClass(MutatorId = "math_is_divisibleby_mutator")]
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
                divisorInput = InputFactory.Create(Define.EConnection.InputValue, DIVISOR_INPUT, Define.EAlign.Left, CHECK);
                mBlock.AppendInput(divisorInput);
            }
            else if (!divisible && divisorInput != null)
            {
                mBlock.RemoveInput(divisorInput);
            }
        }
    }
}
