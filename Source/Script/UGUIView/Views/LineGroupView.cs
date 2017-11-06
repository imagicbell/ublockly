using UnityEngine;

namespace PTGame.Blockly.UGUI
{
    public class LineGroupView : BaseView
    {
        [SerializeField] private float m_ReservedStartX;
        
        public override ViewType Type
        {
            get { return ViewType.LineGroup; }
        }
        
        /// <summary>
        /// Set the reserved x start pos for other UIs, like mutator entry...
        /// </summary>
        public float ReservedStartX
        {
            get { return m_ReservedStartX; }
            set { m_ReservedStartX = value; }
        }

        private float mMarginLeft
        {
            get { return BlockViewSettings.Get().ContentMargin.left + m_ReservedStartX; }
        }

        private float mMarginRight
        {
            get
            {
                if (Childs == null || Childs.Count == 0)
                    return 0;

                bool applyMargin = true;
                
                //don't apply right margin when the last input connection is not a slot
                InputView inputView = Childs[Childs.Count - 1] as InputView;
                if (inputView != null)
                {
                    ConnectionInputView conView = inputView.GetConnectionView();
                    if (conView != null && !conView.IsSlot)
                        applyMargin = false;
                }
                return applyMargin ? BlockViewSettings.Get().ContentMargin.right : 0;
            }
        }

        private float mMarginTop
        {
            get
            {
                if (Childs == null || Childs.Count == 0)
                    return 0;

                for (int i = 0; i < Childs.Count; i++)
                {
                    InputView inputView = Childs[i] as InputView;
                    if (inputView != null)
                    {
                        ConnectionInputView conView = inputView.GetConnectionView();
                        if (conView != null && conView.IsSlot)
                            return BlockViewSettings.Get().ContentMargin.top;
                    }
                }
                return 0;
            }
        }

        private float mMarginBottom
        {
            get
            {
                if (Childs == null || Childs.Count == 0)
                    return 0;

                for (int i = 0; i < Childs.Count; i++)
                {
                    InputView inputView = Childs[i] as InputView;
                    if (inputView != null)
                    {
                        ConnectionInputView conView = inputView.GetConnectionView();
                        if (conView != null && (conView.IsSlot || conView.ConnectionType == ConnectionView.ConType.NEXT_STATEMENT))
                            return BlockViewSettings.Get().ContentMargin.bottom;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// consider if apply margin
        /// </summary>
        public override Vector2 ChildStartXY
        {
            get { return new Vector2(mMarginLeft, -mMarginTop); }
        }

        protected override Vector2 CalculateSize()
        {
            //accumulate size of all inputs
            Vector2 size = Vector2.zero;
            for (int i = 0; i < Childs.Count; i++)
            {
                size.x += Childs[i].Width;
                if (i < Childs.Count - 1)
                    size.x += BlockViewSettings.Get().ContentSpace.x;
                
                size.y = Mathf.Max(size.y, Childs[i].Size.y);
            }

            size.x += mMarginLeft + mMarginRight;
            size.y += mMarginTop + mMarginBottom;
            return size;
        }
    }
}