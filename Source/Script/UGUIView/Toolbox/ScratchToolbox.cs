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
    public class ScratchToolbox : BaseToolbox
    {
        [SerializeField] protected GameObject m_MenuItemPrefab;
        [SerializeField] protected RectTransform m_MenuListContent;
        [SerializeField] protected GameObject m_BlockScrollList;
        [SerializeField] protected GameObject m_BlockContentPrefab;
        [SerializeField] protected GameObject m_BinArea;

        protected override void Build()
        {
            BuildMenu();
            mMenuList[mConfig.BlockCategoryList[0].CategoryName].isOn = true;
        }

        protected virtual void BuildMenu()
        {
            foreach (var category in mConfig.BlockCategoryList)
            {
                GameObject menuItem = GameObject.Instantiate(m_MenuItemPrefab, m_MenuListContent, false);
                menuItem.name = category.CategoryName;
                menuItem.GetComponentInChildren<Text>().text = I18n.Get(category.CategoryName);
                Image[] images = menuItem.GetComponentsInChildren<Image>();
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].color = category.Color;
                }
                menuItem.SetActive(true);
                
                Toggle toggle = menuItem.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((selected) =>
                {
                    if (selected)
                        ShowBlockCategory(menuItem.name);
                });
                mMenuList[category.CategoryName] = toggle;
            }
            
            //layout the BlockScrollList
            GridLayoutGroup layoutGroup = m_MenuListContent.GetComponent<GridLayoutGroup>();
            int lineCount = Mathf.CeilToInt(mConfig.BlockCategoryList.Count / 2.0f);
            float height = layoutGroup.padding.vertical + (lineCount - 1) * layoutGroup.spacing.y + lineCount * layoutGroup.cellSize.y;
            Vector2 offset = ((RectTransform) m_BlockScrollList.transform).offsetMax;
            offset.y = m_MenuListContent.anchoredPosition.y - height;
            ((RectTransform) m_BlockScrollList.transform).offsetMax = offset;
        }
        
        public void ShowBlockCategory(string categoryName)
        {
            if (string.Equals(categoryName, mActiveCategory))
                return;
         
            if (!m_BlockScrollList.activeInHierarchy)
                m_BlockScrollList.SetActive(true);
            
            if (!string.IsNullOrEmpty(mActiveCategory))
                mRootList[mActiveCategory].SetActive(false);

            mActiveCategory = categoryName;
            
            GameObject contentObj;
            RectTransform contentTrans;
            if (mRootList.TryGetValue(categoryName, out contentObj))
            {
                contentObj.SetActive(true);
                contentTrans = contentObj.transform as RectTransform;
            }
            else
            {
                contentObj = GameObject.Instantiate(m_BlockContentPrefab, m_BlockContentPrefab.transform.parent);
                contentObj.name = "Content_" + categoryName;
                contentObj.SetActive(true);
                mRootList[categoryName] = contentObj;

                contentTrans = contentObj.GetComponent<RectTransform>();
                
                //build new blocks
                if (categoryName.Equals(Define.VARIABLE_CATEGORY_NAME))
                    BuildVariableBlocks();
                else if (categoryName.Equals(Define.PROCEDURE_CATEGORY_NAME))
                    BuildProcedureBlocks();
                else
                    BuildBlockViewsForActiveCategory();
            }

            m_BlockScrollList.GetComponent<ScrollRect>().content = contentTrans;
        }
        
        public void HideBlockCategory()
        {
            if (string.IsNullOrEmpty(mActiveCategory))
                return;

            mRootList[mActiveCategory].SetActive(false);
            mMenuList[mActiveCategory].isOn = false;
            m_BlockScrollList.SetActive(false);
            mActiveCategory = null;
        }

        /// <summary>
        /// Build block views for the active category, child class should implement this for custom build
        /// </summary>
        protected virtual void BuildBlockViewsForActiveCategory()
        {
            Transform contentTrans = mRootList[mActiveCategory].transform;
            var blockTypes = mConfig.GetBlockCategory(mActiveCategory).BlockList;
            foreach (string blockType in blockTypes)
            {
                NewBlockView(blockType, contentTrans);
            }
        }
        
        public override bool CheckBin(BlockView blockView)
        {
            if (blockView.InToolbox) return false;
            
            RectTransform toggleTrans = m_BinArea.transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(toggleTrans, UnityEngine.Input.mousePosition, BlocklyUI.UICanvas.worldCamera))
            {
                m_BinArea.gameObject.SetActive(true);
                return true;
            }
            m_BinArea.gameObject.SetActive(false);
            return false;
        }

        public override void FinishCheckBin(BlockView blockView)
        {
            if (CheckBin(blockView))
                blockView.Dispose();
            m_BinArea.gameObject.SetActive(false);
        }
    }
}