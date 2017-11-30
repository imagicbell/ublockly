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
        [SerializeField] private Button m_RunBtn;
        [SerializeField] private Toggle m_Bin;
        [SerializeField] private BlockStatusView m_StatusView;
 
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
            m_Toolbox.Init(workspace, ToolboxConfig.Load("classic"));
            
            m_RunBtn.onClick.AddListener(RunCode);
            
            if (workspace.TopBlocks.Count > 0)
                BuildViews();
        }

        public void UnBindModel()
        {
            mWorkspace.Dispose();
            mWorkspace = null;
            
            m_RunBtn.onClick.RemoveAllListeners();
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

        private UnityEvent mRunCodeEvent = null;

        public void AddRunCodeListener(UnityAction listener)
        {
            if (mRunCodeEvent == null)
                mRunCodeEvent = new Button.ButtonClickedEvent();
            mRunCodeEvent.AddListener(listener);
        }

        public void RemoveRunCodeListener(UnityAction listener)
        {
            if (mRunCodeEvent != null)
                mRunCodeEvent.RemoveListener(listener);
        }

        public void RunCode()
        {
            if (mRunCodeEvent != null)
                mRunCodeEvent.Invoke();
            
//            Lua.Interpreter.Run(mWorkspace);
            CSharp.Interpreter.Run(mWorkspace);
            m_StatusView.enabled = true;
        }
        
        /// <summary>
        /// Check current block move is over bin rect
        /// </summary>
        public bool OverBin()
        {
            RectTransform toggleTrans = m_Bin.transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(toggleTrans, UnityEngine.Input.mousePosition, BlocklyUI.UICanvas.worldCamera))
            {
                if (!m_Bin.isOn) m_Bin.isOn = true;
                return true;
            }
            else
            {
                if (m_Bin.isOn) m_Bin.isOn = false;
                return false;
            }
        }

        /// <summary>
        /// reset bin status
        /// </summary>
        public void ResetBin()
        {
            m_Bin.isOn = false;
        }

        /// <summary>
        /// todo: entry
        /// </summary>
        private void Awake()
        {
            Blockly.Dispose();
            Blockly.Init();
            BlocklyUI.NewWorkspace();
        }

        /// <summary>
        /// todo: call in OnDestroy() 
        /// </summary>
        public void Dispose()
        {
            UnBindModel();
            
            BlockViewSettings.Dispose();
            Resources.UnloadUnusedAssets();
        }
    }
}
