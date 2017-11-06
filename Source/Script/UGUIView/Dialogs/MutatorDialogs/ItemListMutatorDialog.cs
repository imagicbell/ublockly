using UnityEngine;
using UnityEngine.UI;

namespace PTGame.Blockly.UGUI
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

            m_ItemCountTitle.text = Blockly.Msg[MsgDefine.TEXT_CREATE_JOIN_ITEM_TITLE_ITEM];
            
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