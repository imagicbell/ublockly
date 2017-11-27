using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldDropdownDialog : FieldDialog
    {
        [SerializeField] private GameObject m_ItemPrefab;

        protected FieldDropdownMenu[] mOptions;
        protected List<Toggle> mToggleItems;
        
        private FieldDropdown mFieldDropdown
        {
            get { return mField as FieldDropdown; }
        }

        protected override void OnInit()
        {
            m_ItemPrefab.GetComponent<Toggle>().group.allowSwitchOff = true;
            
            mOptions = mFieldDropdown.GetOptions();
            mToggleItems = new List<Toggle>();
            for (int i = 0; i < mOptions.Length; i++)
            {
                FieldDropdownMenu option = mOptions[i];
                GameObject itemObj = GameObject.Instantiate(m_ItemPrefab, m_ItemPrefab.transform.parent, false);
                itemObj.SetActive(true);
                
                itemObj.GetComponentInChildren<Text>().text = option.Text;
                Toggle toggle = itemObj.GetComponent<Toggle>();
                if (!string.IsNullOrEmpty(mFieldDropdown.GetText()))
                    toggle.isOn = mFieldDropdown.GetText().Equals(option.Text);
                else toggle.isOn = false;
                
                mToggleItems.Add(toggle);
            }
            
            AddCloseEvent(() =>
            {
                for (int i = 0; i < mToggleItems.Count; i++)
                {
                    if (mToggleItems[i].isOn)
                    {
                        mFieldDropdown.OnItemSelected(i);
                        break;
                    }
                }
            });
        }
    }
}
