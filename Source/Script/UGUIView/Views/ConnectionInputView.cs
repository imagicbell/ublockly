using System;
using UnityEngine;

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
                    return new Vector2(BlockViewSettings.Get().ValueConnectPointRect.width, BlockViewSettings.Get().ContentHeight);
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
                        return new Vector2(Size.x, BlockViewSettings.Get().ContentHeight + BlockViewSettings.Get().ContentMargin.bottom);
                    
                    // calculate the height by adding all child statement blocks' height
                    // todo: width is calculated by aliging right
                    Vector2 size = new Vector2(Size.x, 0);
                    
                    bool addConnectPointSpace = true;
                    BlockView nextView = mTargetBlockView;
                    while (nextView != null)
                    {
                        ConnectionView nextCon = nextView.GetConnectionView(Define.EConnection.NextStatement);
                        if (nextCon == null)
                        {
                            addConnectPointSpace = false;
                            break;
                        }
                        
                        size.y += nextView.Height;
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
