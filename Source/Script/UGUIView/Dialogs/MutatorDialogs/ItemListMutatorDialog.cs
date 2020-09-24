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

using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class ItemListMutatorDialog : BaseDialog
    {
        [SerializeField] private Slider m_ItemCountSlider;
        [SerializeField] private Text m_ItemCountText;
        [SerializeField] private Text m_ItemCountTitle;

        private ItemListMutator mItemListMutator
        {
            get { return mBlock.Mutator as ItemListMutator; }
        }

        protected override void OnInit()
        {
            m_ItemCountSlider.value = mItemListMutator.ItemCount;
            m_ItemCountText.text = mItemListMutator.ItemCount.ToString();

            m_ItemCountTitle.text = I18n.Get(MsgDefine.TEXT_CREATE_JOIN_ITEM_TITLE_ITEM);
            
            AddCloseEvent(() =>
            {
                mItemListMutator.Mutate((int) m_ItemCountSlider.value);
            });
            
            m_ItemCountSlider.onValueChanged.AddListener((value) =>
            {
                m_ItemCountText.text = ((int) value).ToString();
            });
        }
    }
}
