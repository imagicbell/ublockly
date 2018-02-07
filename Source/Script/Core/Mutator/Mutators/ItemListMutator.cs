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


using System.Xml;
using System.Collections.Generic;

namespace UBlockly
{
    [MutatorClass(MutatorId = "text_join_mutator;lists_create_with_item_mutator")]
    public class ItemListMutator : Mutator
    {
        private const string EMPTY_NAME = "EMPTY";
        private const string ADD_INPUT_PREFIX = "ADD";
        
        private int mItemCount = 2;
        public int ItemCount { get { return mItemCount; } }

        private string mLabelText;

        public override bool NeedEditor
        {
            get { return true; }
        }
        
        public void Mutate(int itemCount)
        {
            if (mItemCount == itemCount)
                return;
            
            mItemCount = itemCount;
            if (mBlock != null)
                UpdateInternal();
        }
        
        public override XmlElement ToXml()
        {
            XmlElement xmlElement = XmlUtil.CreateDom("mutation");
            xmlElement.SetAttribute("items", mItemCount.ToString());
            return xmlElement;
        }

        public override void FromXml(XmlElement xmlElement)
        {
            mItemCount = int.Parse(xmlElement.GetAttribute("items"));
            UpdateInternal();
        }

        protected override void OnAttached()
        {
            Input defaultInput = mBlock.InputList[0];
            defaultInput.SetName(EMPTY_NAME);
            FieldLabel field = defaultInput.FieldRow[0] as FieldLabel;
            mLabelText = field.GetText();
            UpdateInternal();
        }

        private void UpdateInternal()
        {
            // currently reserve the dummy input, it will only show the Label Field on UI
            Input emptyInput = mBlock.GetInput(EMPTY_NAME);
            if (mItemCount > 0 && emptyInput != null)
            {
                mBlock.RemoveInput(emptyInput);
            }
            else if (mItemCount == 0 && emptyInput == null)
            {
                emptyInput = InputFactory.Create(Define.EConnection.DummyInput, EMPTY_NAME, Define.EAlign.Right, null);
                emptyInput.AppendField(new FieldLabel(null, mLabelText));
                mBlock.AppendInput(emptyInput);
            }

            //add new inputs
            int i = 0;
            for (i = 0; i < mItemCount; i++)
            {
                Input addInput = mBlock.GetInput("ADD" + i);
                if (addInput == null)
                {
                    addInput = InputFactory.Create(Define.EConnection.InputValue, ADD_INPUT_PREFIX + i, Define.EAlign.Right, null);
                    mBlock.AppendInput(addInput);
                }
                if (i == 0)
                {
                    if (mBlock.GetField("Title") == null)
                        addInput.AppendField(new FieldLabel("Title", mLabelText));
                }
            }

            // remove deleted inputs
            while (true)
            {
                Input addInput = mBlock.GetInput("ADD" + i);
                if (addInput == null)
                    break;

                mBlock.RemoveInput(addInput);
                i++;
            }
        }
    }
}
