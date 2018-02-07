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
using System.Linq;
using System.Xml;

namespace UBlockly
{
    /// <summary>
    /// mutator for index at FROM_START, FROM_END, FIRST, END...
    /// support multiple "at"s, at, at1, at2...
    /// </summary>
    [MutatorClass(MutatorId = "index_at_mutator")]
    public class IndexAtMutator : Mutator
    {
        private static List<string> CHECK = new List<string> {"Number"}; 

        private class AtData
        {
            internal FieldDropdown whereDropdown;
            //keep record the index of AT input in input list, for later use to reconstruct the AT input
            internal int atInputIndex;
            internal string xmlAttr;
            internal string inputName;
        }

        private List<AtData> mAtDatas = new List<AtData>();
        private bool mIsOnly = true;
        private MemorySafeMutatorObserver mObserver;
        
        private bool IsAt(string dropdownValue)
        {
            return "FROM_START".Equals(dropdownValue) || "FROM_END".Equals(dropdownValue);
        }
        
        public override bool NeedEditor
        {
            get { return false; }
        }
        
        protected override void MutateInternal()
        {
            if (mBlock != null)
                UpdateInternal();
        }

        protected override void OnAttached()
        {
            if (mBlock.GetField("WHERE") != null)
            {
                AtData data = new AtData();
                data.whereDropdown = mBlock.GetField("WHERE") as FieldDropdown;
                data.atInputIndex = mBlock.InputList.FindIndex(input => "AT".Equals(input.Name));
                data.xmlAttr = "at";
                data.inputName = "AT";
                mAtDatas.Add(data);

                mIsOnly = true;
            }
            else if (mBlock.GetField("WHERE1") != null)
            {
                int i = 1;
                while (true)
                {
                    FieldDropdown dropdown = mBlock.GetField("WHERE" + i) as FieldDropdown;
                    if (dropdown == null)
                        break;
                    AtData data = new AtData();
                    data.whereDropdown = dropdown;
                    data.atInputIndex = mBlock.InputList.FindIndex(input => ("AT" + i).Equals(input.Name));
                    data.xmlAttr = "at" + i;
                    data.inputName = "AT" + i;
                    mAtDatas.Add(data);
                    i++;
                }

                mIsOnly = false;
            }
            else
            {
                throw new Exception("FieldDropDown \"WHERE\" not found.");
            }

            UpdateInternal();

            //register observer
            mObserver = new MemorySafeMutatorObserver(this);
            foreach (AtData atData in mAtDatas)
            {
                atData.whereDropdown.AddObserver(mObserver);
            }
        }
        
        protected override void OnDetached()
        {
            //remove observer
            foreach (AtData atData in mAtDatas)
            {
                atData.whereDropdown.RemoveObserver(mObserver);
            }
        }

        public override XmlElement ToXml()
        {
            XmlElement xmlElement = XmlUtil.CreateDom("mutation");
            if (mIsOnly)
            {
                bool isAt = IsAt(mAtDatas[0].whereDropdown.GetValue());
                xmlElement.SetAttribute("at", isAt ? "true" : "false");
            }
            else
            {
                foreach (AtData data in mAtDatas)
                {
                    bool isAt = IsAt(data.whereDropdown.GetValue());
                    xmlElement.SetAttribute(data.xmlAttr, isAt ? "true" : "false");
                }
            }
            return xmlElement;
        }

        public override void FromXml(XmlElement xmlElement)
        {
            List<bool> ats = new List<bool>();
            if (mIsOnly)
            {
                ats.Add(xmlElement.GetAttribute("at") == "true");
            }
            else
            {
                foreach (AtData data in mAtDatas)
                {
                    ats.Add(xmlElement.GetAttribute(data.xmlAttr) == "true");
                }
            }
            UpdateInternal(ats);
        }

        protected void UpdateInternal()
        {
            List<bool> isAts = mAtDatas.Select(data => IsAt(data.whereDropdown.GetValue())).ToList();
            UpdateInternal(isAts);
        }
        
        protected void UpdateInternal(List<bool> isAts)
        {
            for (int i = 0; i < isAts.Count; i++)
            {
                bool isAt = isAts[i];
                AtData data = mAtDatas[i];
                Input atInput = mBlock.GetInput(data.inputName);
                if (isAt)
                {
                    if (atInput == null)
                    {
                        atInput = InputFactory.Create(Define.EConnection.InputValue, data.inputName, Define.EAlign.Left, CHECK);
                        mBlock.AppendInput(atInput, data.atInputIndex);
                    }
                    else if (atInput.Type == Define.EConnection.DummyInput)
                    {
                        //remove dummy input first
                        mBlock.RemoveInput(atInput);
                        atInput = InputFactory.Create(Define.EConnection.InputValue, data.inputName, Define.EAlign.Left, CHECK);
                        mBlock.AppendInput(atInput, data.atInputIndex);
                    }
                }
                else
                {
                    if (atInput != null)
                    {
                        mBlock.RemoveInput(atInput);
                    }
                }
            }
        }
    }
}
