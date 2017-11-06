using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PTGame.Blockly.UGUI
{
    public class ToolboxView : MonoBehaviour
    {
        [SerializeField] private Button m_HideBtn;
        [SerializeField] private GameObject m_MenuItemPrefab;
        [SerializeField] private RectTransform m_MenuListContent;
        [SerializeField] private GameObject m_BlockScrollList;
        [SerializeField] private GameObject m_BlockContentPrefab;

        /// <summary>
        /// the current selected block category name
        /// </summary>
        private string mSelectedMenu;

        /// <summary>
        /// different scroll content for different block category
        /// </summary>
        private Dictionary<string, GameObject> mBlockContents = new Dictionary<string, GameObject>();
        /// <summary>
        /// different toggle item for different block category
        /// </summary>
        private Dictionary<string, Toggle> mMenuList = new Dictionary<string, Toggle>();

        private Workspace mWorkspace;
        
        /// <summary>
        /// Call on start, build toolbox from workspace model data
        /// </summary>
        public void Init(Workspace workspace)
        {
            mWorkspace = workspace;
            mWorkspace.VariableMap.AddObserver(new VariableObserver());
            mWorkspace.ProcedureDB.AddObserver(new ProcedureObserver());
            
            Dictionary<string, List<string>> categories = BlockFactory.Instance.GetCategories();
            foreach (var categoryName in categories.Keys)
            {
                GameObject menuItem = GameObject.Instantiate(m_MenuItemPrefab, m_MenuListContent, false);
                menuItem.name = categoryName;
                menuItem.GetComponentInChildren<Text>().text = categoryName.ToUpperInvariant();
                Image[] images = menuItem.GetComponentsInChildren<Image>();
                for (int i = 0; i < images.Length; i++)
                {
                    try
                    {
                        string colorHue = Blockly.Msg[categoryName.ToUpper() + "_HUE"];
                        images[i].color = Color.HSVToRGB(int.Parse(colorHue) / 360f, 1, 1);
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Color {0}_HUE is not defined.", categoryName.ToUpper());
                        throw;
                    }
                }
                menuItem.SetActive(true);
                
                Toggle toggle = menuItem.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((selected) =>
                {
                    if (selected)
                        SelectBlockMenu(menuItem.name);
                });
                mMenuList[categoryName] = toggle;
            }

            m_HideBtn.gameObject.SetActive(false);
            m_HideBtn.onClick.AddListener(HideBlockMenu);
        }

        /// <summary>
        /// Create a new block view in toolbox 
        /// </summary>
        public BlockView NewBlockView(string blockType, Transform parent, int index = -1)
        {
            Block block = mWorkspace.NewBlock(blockType);
            return NewBlockView(block, parent, index);
        }

        /// <summary>
        /// Create a new block view in toolbox 
        /// </summary>
        public BlockView NewBlockView(Block block, Transform parent, int index = -1)
        {
            mWorkspace.RemoveTopBlock(block);
            
            BlockView view = BlockViewFactory.Get().CreateView(block);
            view.InToolbox = true;
            view.ViewTransform.SetParent(parent, false);
            ToolboxBlockMask.AddMask(view);

            if (index >= 0)
                view.ViewTransform.SetSiblingIndex(index);
            
            return view;
        }

        public void SelectBlockMenu(string categoryName)
        {
            if (categoryName.Equals(mSelectedMenu))
                return;
         
            if (!m_BlockScrollList.activeInHierarchy)
                m_BlockScrollList.SetActive(true);
            
            if (!string.IsNullOrEmpty(mSelectedMenu))
                mBlockContents[mSelectedMenu].SetActive(false);

            mSelectedMenu = categoryName;
            
            GameObject contentObj;
            RectTransform contentTrans;
            if (mBlockContents.TryGetValue(mSelectedMenu, out contentObj))
            {
                contentObj.SetActive(true);
                contentTrans = contentObj.transform as RectTransform;
            }
            else
            {
                contentObj = GameObject.Instantiate(m_BlockContentPrefab, m_BlockContentPrefab.transform.parent);
                contentObj.name = "Content_" + mSelectedMenu;
                contentObj.SetActive(true);
                mBlockContents[mSelectedMenu] = contentObj;

                contentTrans = contentObj.GetComponent<RectTransform>();
                
                //build new blocks
                if (categoryName.Equals(Blockly.BLOCK_CATEGORY_NAME_VARIABLE))
                {
                    BuildVariableBlocks();
                }
                else if (categoryName.Equals(Blockly.BLOCK_CATEGORY_NAME_PROCEDURE))
                {
                    BuildProcedureBlocks();
                }
                else
                {
                    List<string> blockTypes = BlockFactory.Instance.GetCategories()[mSelectedMenu];
                    foreach (string blockType in blockTypes)
                    {
                        NewBlockView(blockType, contentTrans);
                    }  
                }
            }

            //resize the background
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTrans);
            m_BlockScrollList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, LayoutUtility.GetPreferredWidth(contentTrans));
            
            m_BlockScrollList.GetComponent<ScrollRect>().content = contentTrans;
            m_HideBtn.gameObject.SetActive(true);
        }

        public void HideBlockMenu()
        {
            if (string.IsNullOrEmpty(mSelectedMenu))
                return;
            
            mBlockContents[mSelectedMenu].SetActive(false);
            mMenuList[mSelectedMenu].isOn = false;
            m_HideBtn.gameObject.SetActive(false);
            m_BlockScrollList.SetActive(false);
            mSelectedMenu = null;
        }
        
        #region Variables
        
        private Dictionary<string, BlockView> mVariableGetterViews = new Dictionary<string, BlockView>();
        private List<BlockView> mVariableHelperViews = new List<BlockView>();
        
        private void BuildVariableBlocks()
        {
            Transform parent = mBlockContents[Blockly.BLOCK_CATEGORY_NAME_VARIABLE].transform;
            
            //build createVar button
            GameObject obj = GameObject.Instantiate(BlockViewSettings.Get().PrefabBtnCreateVar);
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                DialogFactory.Get().CreateDialog("variable_name");
            });

            List<VariableModel> allVars = mWorkspace.GetAllVariables();
            if (allVars.Count == 0) return;
            
            CreateVariableHelperViews();

            //list all variable getter views
            foreach (VariableModel variable in mWorkspace.GetAllVariables())
            {
                CreateVariableGetterView(variable.Name);
            }
        }

        private void CreateVariableGetterView(string varName)
        {
            if (mVariableGetterViews.ContainsKey(varName))
                return;

            GameObject parentObj;
            if (!mBlockContents.TryGetValue(Blockly.BLOCK_CATEGORY_NAME_VARIABLE, out parentObj))
                return;

            Block block = mWorkspace.NewBlock(Blockly.VARIABLE_GET_BLOCK_TYPE);
            block.SetFieldValue("VAR", varName);
            BlockView view = NewBlockView(block, parentObj.transform);
            mVariableGetterViews[varName] = view;
        }

        private void DeleteVariableGetterView(string varName)
        {
            BlockView view;
            mVariableGetterViews.TryGetValue(varName, out view);
            if (view != null)
            {
                mVariableGetterViews.Remove(varName);
                view.Dispose();
            }
        }
        
        private void CreateVariableHelperViews()
        {
            GameObject parentObj;
            if (!mBlockContents.TryGetValue(Blockly.BLOCK_CATEGORY_NAME_VARIABLE, out parentObj))
                return;
            
            string varName = mWorkspace.GetAllVariables()[0].Name;
            List<string> blockTypes = BlockFactory.Instance.GetCategories()[Blockly.BLOCK_CATEGORY_NAME_VARIABLE];
            foreach (string blockType in blockTypes)
            {
                if (!blockType.Equals(Blockly.VARIABLE_GET_BLOCK_TYPE))
                {
                    Block block = mWorkspace.NewBlock(blockType);
                    block.SetFieldValue("VAR", varName);
                    BlockView view = NewBlockView(block, parentObj.transform);
                    mVariableHelperViews.Add(view);
                }
            }
        }

        private void DeleteVariableHelperViews()
        {
            foreach (BlockView view in mVariableHelperViews)
            {
                view.Dispose();
            }
            mVariableHelperViews.Clear();
        }

        private void OnVariableUpdate(VariableUpdateData updateData)
        {
            switch (updateData.Type)
            {
                case VariableUpdateData.Create:
                {
                    if (mVariableHelperViews.Count == 0)
                        CreateVariableHelperViews();
                    CreateVariableGetterView(updateData.VarName);
                    break;
                }
                case VariableUpdateData.Delete:
                {
                    DeleteVariableGetterView(updateData.VarName);
                    
                    //change variable helper view
                    List<VariableModel> allVars = mWorkspace.GetAllVariables();
                    if (allVars.Count == 0)
                    {
                        DeleteVariableHelperViews();
                    }
                    else
                    {
                        foreach (BlockView view in mVariableHelperViews)
                        {
                            if (view.Block.GetFieldValue("VAR").Equals(updateData.VarName))
                            {
                                view.Block.SetFieldValue("VAR", allVars[0].Name);
                            }
                        }
                    }
                    break;
                }
            }
        }

        private class VariableObserver : IObserver<VariableUpdateData>
        {
            public void OnUpdated(object subject, VariableUpdateData args)
            {
                BlocklyUI.WorkspaceView.Toolbox.OnVariableUpdate(args);
            }
        }
        #endregion

        #region Procedures
        
        private Dictionary<string, BlockView> mProcedureCallerViews = new Dictionary<string, BlockView>();
        
        private void BuildProcedureBlocks()
        {
            Transform parent = mBlockContents[Blockly.BLOCK_CATEGORY_NAME_PROCEDURE].transform;
            List<string> blockTypes = BlockFactory.Instance.GetCategories()[Blockly.BLOCK_CATEGORY_NAME_PROCEDURE];
            foreach (string blockType in blockTypes)
            {
                if (!blockType.Equals(Blockly.CALL_NO_RETURN_BLOCK_TYPE) &&
                    !blockType.Equals(Blockly.CALL_WITH_RETURN_BLOCK_TYPE))
                {
                    NewBlockView(blockType, parent);
                }
            }
            
            // list all caller views
            foreach (Block block in mWorkspace.ProcedureDB.GetDefinitionBlocks())
            {
                CreateProcedureCallerView(((ProcedureDefinitionMutator) block.Mutator).ProcedureInfo, ProcedureDB.HasReturn(block));
            }
        }

        private void CreateProcedureCallerView(Procedure procedureInfo, bool hasReturn)
        {
            if (mProcedureCallerViews.ContainsKey(procedureInfo.Name))
                return;
            
            GameObject parentObj;
            if (!mBlockContents.TryGetValue(Blockly.BLOCK_CATEGORY_NAME_PROCEDURE, out parentObj))
                return;

            string blockType = hasReturn ? Blockly.CALL_WITH_RETURN_BLOCK_TYPE : Blockly.CALL_NO_RETURN_BLOCK_TYPE;
            Block block = mWorkspace.NewBlock(blockType);
            block.SetFieldValue("NAME", procedureInfo.Name);
            BlockView view = NewBlockView(block, parentObj.transform);
            mProcedureCallerViews[procedureInfo.Name] = view;
        }
        
        private void DeleteProcedureCallerView(Procedure procedureInfo)
        {
            BlockView view;
            mProcedureCallerViews.TryGetValue(procedureInfo.Name, out view);
            if (view != null)
            {
                mProcedureCallerViews.Remove(procedureInfo.Name);
                view.Dispose();
            }
        }
        
        private void OnProcedureUpdate(ProcedureUpdateData updateData)
        {
            switch (updateData.Type)
            {
                case ProcedureUpdateData.Add:
                {
                    CreateProcedureCallerView(updateData.ProcedureInfo, ProcedureDB.HasReturn(updateData.ProcedureDefinitionBlock));
                    break;
                }
                case ProcedureUpdateData.Remove:
                {
                    DeleteProcedureCallerView(updateData.ProcedureInfo);
                    break;
                }
                case ProcedureUpdateData.Mutate:
                {
                    //mutate the caller prototype view
                    BlockView view = mProcedureCallerViews[updateData.ProcedureInfo.Name];
                    if (!updateData.ProcedureInfo.Name.Equals(updateData.NewProcedureInfo.Name))
                    {
                        mProcedureCallerViews.Remove(updateData.ProcedureInfo.Name);
                        mProcedureCallerViews[updateData.NewProcedureInfo.Name] = view;
                    }
                    
                    ((ProcedureMutator) view.Block.Mutator).Mutate(updateData.NewProcedureInfo);
                    break;
                }
            }
        }
        
        private class ProcedureObserver : IObserver<ProcedureUpdateData>
        {
            public void OnUpdated(object subject, ProcedureUpdateData args)
            {
                BlocklyUI.WorkspaceView.Toolbox.OnProcedureUpdate(args);
            }
        }
        
        #endregion
    }
}