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


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class WorkspaceView : MonoBehaviour
    {
        [SerializeField] private BaseToolbox m_Toolbox;
        [SerializeField] private RectTransform m_CodingArea;
        [SerializeField] private BlockStatusView m_StatusView;
        [SerializeField] private PlayControlView m_PlayControlView;
 
        public BaseToolbox Toolbox
        {
            get { return m_Toolbox; }
        }

        public RectTransform CodingArea
        {
            get { return m_CodingArea; }
        }

        private Workspace mWorkspace;
        public Workspace Workspace { get { return mWorkspace; } }
        
        /// <summary>
        /// all block gameobject created currently
        /// </summary>
        private Dictionary<string, BlockView> mBlockViews = new Dictionary<string, BlockView>();
        
        public void BindModel(Workspace workspace)
        {
            if (mWorkspace != null)
                UnBindModel();
            
            mWorkspace = workspace;

            RectTransform codingAreaTrans = m_CodingArea.GetComponentInParent<ScrollRect>().transform as RectTransform;
            codingAreaTrans.offsetMin = new Vector2(((RectTransform) m_Toolbox.transform).sizeDelta.x, codingAreaTrans.offsetMin.y);
            
            m_Toolbox.Init(workspace, ToolboxConfig.Load());
            m_PlayControlView.Init(this);
            
            if (workspace.TopBlocks.Count > 0)
                BuildViews();
        }

        public void UnBindModel()
        {
            m_PlayControlView.Reset();
            
            mWorkspace.Dispose();
            mWorkspace = null;
        }
        
        #region manage block views

        public BlockView GetBlockView(Block block)
        {
            BlockView view;
            if (mBlockViews.TryGetValue(block.ID, out view))
                return view;
            return null;
        }

        public void AddBlockView(BlockView blockView)
        {
            mBlockViews[blockView.Block.ID] = blockView;
        }

        public void RemoveBlockView(BlockView blockView)
        {
            mBlockViews.Remove(blockView.Block.ID);
        }
        
        /// <summary>
        /// Clone the block view, and all its child block views
        /// </summary>
        public BlockView CloneBlockView(BlockView blockView, Vector2 xyPos)
        {
            if (blockView.Block == null)
            {
                Debug.LogError("CloneBlockView: the block model is null");
                return null;
            }
            
            Block newBlock = blockView.Block.Clone();
            newBlock.XY = xyPos;
            BlockView newView = BuildBlockView(newBlock);
            return newView;
        }

        /// <summary>
        /// reconstruct workspace ui
        /// </summary>
        public void BuildViews()
        {
            List<Block> topBlocks = mWorkspace.GetTopBlocks(false);
            foreach (Block block in topBlocks)
            {
                BuildBlockView(block);
            }
        }

        private BlockView BuildBlockView(Block block)
        {
            BlockView view = BlockViewFactory.CreateView(block);
            view.InToolbox = false;
            view.ViewTransform.SetParent(m_CodingArea, false);
            view.XY = block.XY;

            foreach (Block childBlock in block.ChildBlocks)
            {
                BuildBlockView(childBlock);
             
                Connection connection = null;
                if (childBlock.PreviousConnection != null)
                    connection = childBlock.PreviousConnection.TargetConnection;
                else if (childBlock.OutputConnection != null)
                    connection = childBlock.OutputConnection.TargetConnection;
                if (connection != null)
                    connection.FireUpdate(Connection.UpdateState.Connected);
            }
            return view;
        }

        /// <summary>
        /// clean workspace ui
        /// </summary>
        public void CleanViews()
        {
            List<Block> topBlocks = mWorkspace.GetTopBlocks(false);
            foreach (Block block in topBlocks)
            {
                GetBlockView(block).Dispose();
            }
        }
        
        #endregion

        /// <summary>
        /// entry
        /// </summary>
        private void Awake()
        {
            Blockly.Dispose();
            Blockly.Init();
            BlocklyUI.NewWorkspace();
        }

        private void OnDestroy()
        {
            BlocklyUI.DestroyWorkspace();
        }

        public void Dispose()
        {
            UnBindModel();
            
            BlockViewSettings.Dispose();
            Resources.UnloadUnusedAssets();
        }
    }
}
