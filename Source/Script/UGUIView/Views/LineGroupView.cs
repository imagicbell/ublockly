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

namespace UBlockly.UGUI
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
                        if (conView != null && conView.IsSlot)
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
                //calculate x: get the last child's right, as input view may align right
                if (i == Childs.Count - 1)
                    size.x = Childs[i].XY.x + Childs[i].Width;
                
                size.y = Mathf.Max(size.y, Childs[i].Height);
            }

            size.x += mMarginRight;
            size.y += mMarginTop + mMarginBottom;
            return size;
        }

        /// <summary>
        /// update all child inputviews to align right
        /// </summary>
        /// <param name="width"></param>
        public void UpdateAlignRight(float width)
        {
            if (Mathf.Approximately(this.Width, width))
                return;
            this.Width = width;
            
            ConnectionInputView conView = ((InputView) LastChild).GetConnectionView();
            if (conView != null && conView.ConnectionInputViewType == ConnectionInputViewType.Statement)
            {
                conView.Width = width - (LastChild.XY.x + conView.XY.x);
            }
            else
            {
                float startX = this.XY.x + width;
                for (int i = Childs.Count - 1; i >= 0; i--)
                {
                    InputView inputView = Childs[i] as InputView;
                    if (i < Childs.Count - 1)
                        startX -= BlockViewSettings.Get().ContentSpace.x;
                    startX -= inputView.Width;
                    inputView.XY = new Vector2(startX, inputView.XY.y);
                }
            }
        }

        /// <summary>
        /// Get the size for drawing background
        /// </summary>
        public Vector2 GetDrawSize()
        {
            //I know it is a little bit awkward here...
            //but to make it seem prettier...maybe refactor later
            Vector2 size = Size;
            ConnectionInputView conView = ((InputView) LastChild).GetConnectionView();
            if (conView != null && !conView.IsSlot)
                size.x -= conView.Width;
            return size;
        }
    }
}
