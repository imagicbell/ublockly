/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
****************************************************************************/
using System.Xml;
using System.Collections.Generic;

namespace UBlockly
{
    public class ItemListMutator : Mutator
    {
        private const string EMPTY_NAME = "EMPTY";
        private const string ADD_INPUT_PREFIX = "ADD";
        
        private int mItemCount = 2;
        public int ItemCount { get { return mItemCount; } }

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
            UpdateInternal();
        }

        private void UpdateInternal()
        {
            /* currently reserve the dummy input, it will only show the Label Field on UI
            Input emptyInput = mBlock.GetInput(EMPTY_NAME);
            if (mItemCount > 0 && emptyInput != null)
            {
                mBlock.RemoveInput(emptyInput);
            }
            else if (mItemCount == 0 && emptyInput == null)
            {
                emptyInput = InputFactory.Create(Blockly.DUMMY_INPUT, EMPTY_NAME, Blockly.ALIGN_LEFT, null);
                mBlock.AppendInput(emptyInput);
            }*/

            //add new inputs
            int i = 0;
            for (i = 0; i < mItemCount; i++)
            {
                if (!mBlock.HasInput("ADD" + i))
                {
                    Input valueInput = InputFactory.Create(Blockly.INPUT_VALUE, ADD_INPUT_PREFIX + i, Blockly.ALIGN_LEFT, null);
                    mBlock.AppendInput(valueInput);
                }
            }

            // remove deleted inputs
            while (true)
            {
                Input atInput = mBlock.GetInput("ADD" + i);
                if (atInput == null)
                    break;

                mBlock.RemoveInput(atInput);
                i++;
            }
        }
    }
}
