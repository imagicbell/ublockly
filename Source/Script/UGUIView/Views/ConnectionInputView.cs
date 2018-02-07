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
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public enum ConnectionInputViewType
    {
        Value = 0,    //value input on the end of one line group
        ValueSlot,    //value slot on anywhere of the line group. Can't exist the same time with "Value" on one linegroup
        Statement,    //Statement input on the end of one line group
    }
    
    public class ConnectionInputView : ConnectionView
    {
        [SerializeField] private ConnectionInputViewType m_ConnectionInputViewType;
        [SerializeField] private Image m_BgImage;
        [SerializeField] private Vector2 m_ImageMeshOffset;

        public override ViewType Type
        {
            get { return ViewType.ConnectionInput; }
        }

        public ConnectionInputViewType ConnectionInputViewType
        {
            get { return m_ConnectionInputViewType; }
            set { m_ConnectionInputViewType = value; }
        }

        /// <summary>
        /// Check if this is a slot input connection view
        /// if true, the block height will expand to BlockViewSettings.Height
        /// if false, the block height wiil narrow to BlockViewSettings.FieldHeight
        /// </summary>
        public bool IsSlot
        {
            get { return m_ConnectionInputViewType == ConnectionInputViewType.ValueSlot; }
        }

        public Image BgImage
        {
            get { return m_BgImage; }
        }

        public override void InitComponents()
        {
            base.InitComponents();
            if (m_BgImage == null)
            {
                m_BgImage = GetComponent<Image>();
                if (m_BgImage == null)
                    throw new Exception("the background image of ConnectionInputView must be a \"CustomMeshImage\"");
            }
        }
       
        public override Vector2 ChildStartXY
        {
            get
            {
                if (m_ConnectionInputViewType == ConnectionInputViewType.Value ||
                    m_ConnectionInputViewType == ConnectionInputViewType.ValueSlot)
                    return new Vector2(BlockViewSettings.Get().ValueConnectPointRect.width, 0);
                return Vector2.zero;
            }
        }

        protected override Vector2 CalculateSize()
        {
            switch (m_ConnectionInputViewType)
            {
                case ConnectionInputViewType.Value:
                {
                    //width is not concerned
                    Vector2 size = new Vector2(BlockViewSettings.Get().ValueConnectPointRect.width, 0);
                    if (mTargetBlockView == null)
                        size.y = BlockViewSettings.Get().ContentHeight;
                    else
                        size.y = mTargetBlockView.Height;
                    return size;
                }
                case ConnectionInputViewType.ValueSlot:
                {
                    if (mTargetBlockView == null)
                        return new Vector2(BlockViewSettings.Get().MinUnitWidth, BlockViewSettings.Get().ContentHeight);
                    Vector2 size = new Vector2(BlockViewSettings.Get().ValueConnectPointRect.width + mTargetBlockView.Width, mTargetBlockView.Height);
                    return size;
                }
                case ConnectionInputViewType.Statement:
                {
                    if (mTargetBlockView == null)
                        return new Vector2(70, BlockViewSettings.Get().ContentHeight + BlockViewSettings.Get().ContentMargin.bottom);
                    
                    // calculate the height by adding all child statement blocks' height
                    Vector2 size = new Vector2(70, 0);
                    
                    bool addConnectPointSpace = true;
                    BlockView nextView = mTargetBlockView;
                    while (nextView != null)
                    {
                        size.y += nextView.Height;
                        
                        ConnectionView nextCon = nextView.GetConnectionView(Define.EConnection.NextStatement);
                        if (nextCon == null)
                        {
                            addConnectPointSpace = false;
                            break;
                        }
                        
                        nextView = nextCon.TargetBlockView;
                    }
                    if (addConnectPointSpace)
                        size.y += BlockViewSettings.Get().StatementConnectPointRect.height;

                    size.y += BlockViewSettings.Get().ContentMargin.bottom;
                    return size;
                }
            }
            return Vector2.zero;
        }

        protected internal override void OnSizeUpdated()
        {
            if (m_BgImage is CustomMeshImage)
            {
                Vector4 drawDimension = new Vector4(m_ImageMeshOffset.x, -Height, Width, m_ImageMeshOffset.y);
                ((CustomMeshImage)m_BgImage).SetDrawDimensions(new[] {drawDimension});    
            }
        }

        /// <summary>
        /// INPUT_VALUE: mTargetBlockView
        /// NEXT_STATEMENT: the last block view of the target statement block views chain
        /// </summary>
        public BlockView GetChildLastBlockView()
        {
            if (ConnectionType == Define.EConnection.InputValue)
                return mTargetBlockView;
            
            BlockView nextView = mTargetBlockView;
            while (nextView != null)
            {
                ConnectionView nextCon = nextView.GetConnectionView(Define.EConnection.NextStatement);
                if (nextCon == null) break;
                nextView = nextCon.TargetBlockView;
            }
            return nextView;
        }
    }
}
