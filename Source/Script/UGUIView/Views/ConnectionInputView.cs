using System;
using UnityEngine;

namespace UBlockly.UGUI
{
    public class ConnectionInputView : ConnectionView
    {
        [SerializeField] private bool m_IsSlot = true;

        public override ViewType Type
        {
            get { return ViewType.ConnectionInput; }
        }

        /// <summary>
        /// Check if this is a slot input connection view
        /// if true, the block height will expand to BlockViewSettings.Height
        /// if false, the block height wiil narrow to BlockViewSettings.FieldHeight
        /// </summary>
        public bool IsSlot
        {
            get { return m_IsSlot; }
            set { m_IsSlot = value; }
        }

        public override Vector2 ChildStartXY
        {
            get
            {
                if (ConnectionType == Define.EConnection.InputValue)
                    return new Vector2(BlockViewSettings.Get().ValueConnectPointRect.width, 0);
                return Vector2.zero;
            }
        }

        protected override Vector2 CalculateSize()
        {
            if (mTargetBlockView == null)
                return new Vector2(BlockViewSettings.Get().EmptyInputSlotWidth, BlockViewSettings.Get().ContentHeight);

            // only slot connection input view will accumulate the width by children's width
            Vector2 size = new Vector2(BlockViewSettings.Get().EmptyInputSlotWidth, 0);
            
            if (ConnectionType == Define.EConnection.InputValue)
            {
                // add the half circle's width
                if (m_IsSlot)
                    size.x = BlockViewSettings.Get().ValueConnectPointRect.width + mTargetBlockView.Width;
                size.y = mTargetBlockView.Height;
            }
            else
            {
                // input_statement
                // calculate the input size by adding all child statement blocks' size
                size.x = mTargetBlockView.Width;
                size.y += mTargetBlockView.Height;

                BlockView nextView = mTargetBlockView;
                bool addConnectPointSpace = true;
                while (true)
                {
                    ConnectionView nextCon = nextView.GetConnectionView(Define.EConnection.NextStatement);
                    if (nextCon == null)
                    {
                        addConnectPointSpace = false;
                        break;
                    }
                    nextView = nextCon.TargetBlockView;
                    if (nextView == null) break;

                    size.x = Mathf.Min(size.x, nextView.Width);
                    size.y += nextView.Height;
                }
                if (addConnectPointSpace)
                    size.y += BlockViewSettings.Get().StatementConnectPointRect.height;
            }
            return size;
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
