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
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class BlockView : BaseView, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private List<Image> m_BgImages = new List<Image>();
        
        public override ViewType Type
        {
            get { return ViewType.Block; }
        }
        
        public string BlockType
        {
            get { return mBlock.Type; }
        }

        private Block mBlock;
        public Block Block { get { return mBlock; } }

        public bool InToolbox { get; set; }
        
        private MemorySafeBlockObserver mBlockObserver;
            
        public void BindModel(Block block)
        {
            if (mBlock == block) return;
            if (mBlock != null) UnBindModel();

            mBlock = block;
            BlocklyUI.WorkspaceView.AddBlockView(this);

            mBlockObserver = new MemorySafeBlockObserver(this);
            mBlock.AddObserver(mBlockObserver);

            //bind input and connections
            int inputIndex = 0;
            foreach (BaseView view in Childs)
            {
                if (view.Type == ViewType.Connection)
                {
                    ConnectionView conView = view as ConnectionView;
                    conView.BindModel(mBlock.GetFirstClassConnection(conView.ConnectionType));
                }
                else if (view.Type == ViewType.LineGroup)
                {
                    LineGroupView groupView = view as LineGroupView;
                    foreach (var inputView in groupView.Childs)
                    {
                        ((InputView) inputView).BindModel(mBlock.InputList[inputIndex]);
                        inputIndex++;
                    }
                }
            }

            RegisterUIEvents();
        }

        public void UnBindModel()
        {
            //unbind input and connections
            int inputIndex = 0;
            foreach (BaseView view in Childs)
            {
                if (view.Type == ViewType.Connection)
                {
                    ((ConnectionView) view).UnBindModel();
                }
                else if (view.Type == ViewType.LineGroup)
                {
                    LineGroupView groupView = view as LineGroupView;
                    foreach (var inputView in groupView.Childs)
                    {
                        ((InputView) inputView).UnBindModel();
                        inputIndex++;
                    }
                }
            }
            
            BlocklyUI.WorkspaceView.RemoveBlockView(this);
            mBlock.RemoveObserver(mBlockObserver);
            mBlock = null;
        }

        /// <summary>
        /// Dispose the block model and block view
        /// </summary>
        public void Dispose()
        {
            Block model = mBlock;
            UnBindModel();
            GameObject.Destroy(this.gameObject);
            model.Dispose();
        }

        #region UI Update

        public override Vector2 ChildStartXY
        {
            get
            {
                if (Childs[0].Type == ViewType.Connection)
                {
                    // connection point' start xy is specified
                    Define.EConnection conType = ((ConnectionView) Childs[0]).ConnectionType;
                    switch (conType)
                    {
                        case Define.EConnection.OutputValue:
                            return BlockViewSettings.Get().ValueConnectPointRect.position;

                        case Define.EConnection.PrevStatement:
                        case Define.EConnection.NextStatement:
                            return BlockViewSettings.Get().StatementConnectPointRect.position;
                    }
                }
                return base.ChildStartXY;
            }
        }

        protected override Vector2 CalculateSize()
        {
            bool alignRight = false;
            
            //accumulate all child lineGroups' size
            Vector2 size = Vector2.zero;
            for (int i = 0; i < Childs.Count; i++)
            {
                LineGroupView groupView = Childs[i] as LineGroupView;
                if (groupView != null)
                {
                    size.x = Mathf.Max(size.x, groupView.Size.x);
                    size.y += groupView.Size.y;
                    if (i < Childs.Count - 1)
                        size.y += BlockViewSettings.Get().ContentSpace.y;

                    if (((InputView) groupView.LastChild).AlignRight)
                        alignRight = true;
                }
            }
            
            //collect all child lineGroups' vertices for custom drawing
            List<Vector4> dimensions = new List<Vector4>();
            for (int i = 0; i < Childs.Count; i++)
            {
                LineGroupView groupView = Childs[i] as LineGroupView;
                if (groupView != null)
                {
                    if (alignRight)
                        groupView.UpdateAlignRight(size.x);
                    
                    //linegroup's anchor and pivot both are top-left
                    Vector2 drawSize = groupView.GetDrawSize();
                    dimensions.Add(new Vector4(groupView.XY.x, groupView.XY.y - drawSize.y, groupView.XY.x + drawSize.x, groupView.XY.y));
                }
            }
            
            //update image mesh
            ((CustomMeshImage)m_BgImages[0]).SetDrawDimensions(dimensions.ToArray());
            return size;
        }

        protected internal override void OnXYUpdated()
        {
            if (InToolbox) return;

            mBlock.XY = XY;
            //update all connection's location
            foreach (var view in Childs)
            {
                if (view.Type == ViewType.Connection)
                {
                    view.OnXYUpdated();
                }
                else if (view.Type == ViewType.LineGroup)
                {
                    LineGroupView groupView = view as LineGroupView;
                    foreach (var inputView in groupView.Childs)
                    {
                        ConnectionInputView conInputView = ((InputView) inputView).GetConnectionView();
                        if (conInputView != null)
                            conInputView.OnXYUpdated();
                    }
                }
            }
        }
        
        /// <summary>
        /// Build the layout of the block view from its topmost child
        /// </summary>
        public void BuildLayout()
        {
            BaseView startView = this.GetLineGroup(0).GetTopmostChild();
            startView.UpdateLayout(startView.HeaderXY);
        }

        /// <summary>
        /// Add the background image which needs to keep the same background color
        /// </summary>
        public void AddBgImage(Image image)
        {
            if (image != null && !m_BgImages.Contains(image))
                m_BgImages.Add(image);
        }

        /// <summary>
        /// Change the color of the background images
        /// </summary>
        public void ChangeBgColor(Color color)
        {
            m_BgImages.RemoveAll(bg => bg == null);
            foreach (Image bg in m_BgImages)
            {
                bg.color = color;
            }
        }

        #endregion

        #region UI Interactions
        
        private void RegisterUIEvents()
        {
            //show mutator editor
            var mutatorEntry = ViewTransform.Find("Mutator_entry");
            if (mutatorEntry != null)
            {
                mutatorEntry.GetComponent<Button>().onClick.AddListener(() =>
                    DialogFactory.CreateMutatorDialog(mBlock)
                );
            }
        }

        /// <summary>
        /// put the blockview under the CodingContent, not in the menu, or child of other block views
        /// </summary>
        public void SetOrphan()
        {
            if (InToolbox)
                InToolbox = false;
            ViewTransform.SetParent(BlocklyUI.WorkspaceView.CodingArea);
            ViewTransform.SetAsLastSibling();
        }

        private Vector2 mTouchOffset;
        // closest connection found when dragging
        private Connection mClosestConnection = null;
        // local connection opposite to closest connection found
        private Connection mAttachingConnection = null;
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            mBlock.UnPlug();
            SetOrphan();
            
            //record the touch offset relative to the block transform
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) ViewTransform.parent, UnityEngine.Input.mousePosition,
                                                                    BlocklyUI.UICanvas.worldCamera, out localPos);
            mTouchOffset = XY - localPos;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) ViewTransform.parent, UnityEngine.Input.mousePosition,
                                                                    BlocklyUI.UICanvas.worldCamera, out localPos);
            XY = localPos + mTouchOffset;

            // find the closest connection
            var oldClosest = mClosestConnection;
            mClosestConnection = null;
            mAttachingConnection = null;
            int minRadius = BlockViewSettings.Get().ConnectSearchRange;
            for (int i = 0; i < Childs.Count; i++)
            {
                if (Childs[i].Type != ViewType.Connection)
                    break;
                if (((ConnectionView) Childs[i]).SearchClosest(minRadius, ref mClosestConnection, ref minRadius))
                {
                    mAttachingConnection = ((ConnectionView) Childs[i]).Connection;
                }
            }

            if (oldClosest != mClosestConnection && oldClosest != null)
                oldClosest.FireUpdate(Connection.UpdateState.UnHighlight);

            if (oldClosest != mClosestConnection && mClosestConnection != null)
            {
                mClosestConnection.FireUpdate(Connection.UpdateState.Highlight);
            }
            
            // check over bin
            BlocklyUI.WorkspaceView.Toolbox.CheckBin(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (mClosestConnection != null)
            {
                // attach to closest connection
                mClosestConnection.Connect(mAttachingConnection);
                mClosestConnection.FireUpdate(Connection.UpdateState.UnHighlight);
            }
            // check over bin
            BlocklyUI.WorkspaceView.Toolbox.FinishCheckBin(this);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            //todo: background outline
            /*if (!eventData.dragging && !InToolbox) 
                BlocklyUI.WorkspaceView.CloneBlockView(this, XYInCodingArea + BlockViewSettings.Get().BumpAwayOffset);*/
        }
        
        #endregion

        #region Block State Update

        /// <summary>
        /// Called when the underlying block has been updated.
        /// </summary>
        protected void OnBlockUpdated(Block.UpdateState updateState)
        {
            switch (updateState)
            {
                case Block.UpdateState.Inputs:
                {
                    //rebuild block view's input views
                    BlockViewBuilder.BuildInputViews(mBlock, this);

                    //reupdate layout
                    BuildLayout();
                    
                    //call this once to update the connection DB
                    this.OnXYUpdated();
                    
                    //call this again to change new input views
                    this.ChangeBgColor(m_BgImages[0].color);
                    
                    break;
                }
            }
        }

        private class MemorySafeBlockObserver : IObserver<int>
        {
            private BlockView mViewRef;

            public MemorySafeBlockObserver(BlockView viewRef)
            {
                mViewRef = viewRef;
            }

            public void OnUpdated(object block, int updateStateMask)
            {
                if (mViewRef == null || mViewRef.ViewTransform == null || mViewRef.Block != block)
                {
                    ((Block) block).RemoveObserver(this);
                }
                else
                {
                    foreach (Block.UpdateState state in Enum.GetValues(typeof(Block.UpdateState)))
                    {
                        if (((1 << (int) state) & updateStateMask) != 0)
                        {
                            mViewRef.OnBlockUpdated(state);
                        }
                    }
                }
            }
        }

        #endregion
        
        #region Child View Getter
        
        /// <summary>
        /// Get the connection view of connectionType
        /// output, previous, next connection
        /// </summary>
        public ConnectionView GetConnectionView(Define.EConnection connectionType)
        {
            int i = 0;
            while (i < Childs.Count)
            {
                ConnectionView view = Childs[i] as ConnectionView;
                if (view == null) break;
                if (view.ConnectionType == connectionType)
                    return view;
                i++;
            }
            //Debug.LogFormat("<color=red>Can't find the {0} connection view in block view of {1}.</color>", connectionType, BlockType);
            return null;
        }

        /// <summary>
        /// Get the connection view of input
        /// </summary>
        public ConnectionView GetInputConnectionView(int inputIndex)
        {
            InputView inputView = GetInputView(inputIndex);
            if (inputView != null)
                return inputView.GetConnectionView();
            return null;
        }

        /// <summary>
        /// Get the index's lineGroup child view, index start from 0
        /// </summary>
        public LineGroupView GetLineGroup(int index)
        {
            int i = 0;
            while (i < Childs.Count)
            {
                LineGroupView view = Childs[i] as LineGroupView;
                if (view != null)
                {
                    if (i + index < Childs.Count)
                        return Childs[i + index] as LineGroupView;
                    break;
                }
                i++;
            }
            //Debug.LogFormat("<color=red>Can't find the {0}th lineGroup in block view of {1}.</color>", index, BlockType);
            return null;
        }

        /// <summary>
        /// Get the index's input view, index start from 0
        /// </summary>
        public InputView GetInputView(int index)
        {
            int inputCounter = 0;
            int groupCounter = 0;
            while (groupCounter < Childs.Count)
            {
                LineGroupView view = Childs[groupCounter] as LineGroupView;
                groupCounter++;
                if (view == null) continue;

                if (inputCounter + view.Childs.Count > index)
                    return view.Childs[index - inputCounter] as InputView;

                inputCounter += view.Childs.Count;
            }
            //Debug.LogFormat("<color=red>Can't find the {0}th input view in block view of {1}.</color>", index, BlockType);
            return null;
        }

        public List<InputView> GetInputViews()
        {
            List<InputView> inputViews = new List<InputView>();
            int i = 0;
            while (i < Childs.Count)
            {
                LineGroupView view = Childs[i] as LineGroupView;
                if (view != null && view.HasChild())
                    inputViews.AddRange(view.Childs.Select(v => v as InputView));
                i++;
            }
            return inputViews;
        }

        /// <summary>
        /// Get the index's field view, index start from 0
        /// </summary>
        public FieldView GetFieldView(int index)
        {
            int fieldCounter = 0;
            int groupCounter = 0;
            while (groupCounter < Childs.Count)
            {
                LineGroupView groupView = Childs[groupCounter] as LineGroupView;
                groupCounter++;
                if (groupView == null) continue;

                int inputCounter = 0;
                while (inputCounter < groupView.Childs.Count)
                {
                    InputView inputView = groupView.Childs[inputCounter] as InputView;
                    //the last child view of inputView is ConnectionInputView
                    if (fieldCounter + inputView.Childs.Count - 1 > index)
                        return inputView.Childs[index - fieldCounter] as FieldView;

                    fieldCounter += inputView.Childs.Count - 1;
                    inputCounter++;
                }
            }
            
            //Debug.LogFormat("<color=red>Can't find the {0}th field view in block view of {1}.</color>", index, BlockType);
            return null;
        }
        
        #endregion
    }
}
