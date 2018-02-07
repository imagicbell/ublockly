/****************************************************************************

Abstract class for block's views

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

namespace UBlockly.UGUI
{
    /***************************
     hierarchy of view:
     - Block
       - ConnectionOutput
       - ConnectionPrev  
       - ConnectionNext
         - Block(Next)
         
       - LineGroup
         - Input
           - Field 
           - Field 
           ...
           - ConnectionInput
             - Block(Input)
         - Input
           ...
       - LineGroup
         ...
       ...
       
     - Block
       ...             
    ***************************/
    
    public enum ViewType
    {
        Block,
        LineGroup,    //parent of inputs and fields in one line
        Input,
        Field,
        Connection,       //block's connection point: output, previous, next
        ConnectionInput,  //connecton slot for attaching input block
    }
    
    public abstract class BaseView : MonoBehaviour
    {
        [SerializeField] private RectTransform m_ViewTransform;
        [SerializeField] private BaseView m_Parent;
        [SerializeField] private BaseView m_Previous;
        [SerializeField] private BaseView m_Next;
        [SerializeField] private List<BaseView> m_Childs = new List<BaseView>();    //can't use LinkedList because of unserializable, and slow on indexing

        public RectTransform ViewTransform
        {
            get { return m_ViewTransform; }
        }

        public BaseView Parent
        {
            get { return m_Parent; }
        }

        public BaseView Previous
        {
            get { return m_Previous; }
        }

        public BaseView Next
        {
            get { return m_Next; }
        }

        public int SiblingIndex
        {
            get { return m_Parent != null ? m_Parent.m_Childs.IndexOf(this) : -1; }
        }
        
        public List<BaseView> Childs
        {
            get { return m_Childs; }
        }

        public bool HasChild()
        {
            return m_Childs.Count > 0;
        }

        public BaseView FirstChild
        {
            get { return HasChild() ? m_Childs[0] : null; }
        }

        public BaseView LastChild
        {
            get { return HasChild() ? m_Childs[m_Childs.Count - 1] : null; }
        }

        /// <summary>
        /// Add child view to this view, default add at last
        /// Iterate through all the next views, and add them as well
        /// </summary>
        public void AddChild(BaseView childView, int index = -1)
        {
            if (m_Childs.Contains(childView))
                return;

            index = index >= 0 ? index : m_Childs.Count;
            
            //1. update previous
            BaseView preView = index > 0 ? m_Childs[index - 1] : null;
            if (preView != null)
            {
                preView.m_Next = childView;
                childView.m_Previous = preView;
            }
            
            //2. add iteratively
            BaseView itor = childView;
            do
            {
                m_Childs.Insert(index, itor);
                itor.m_Parent = this;

                if (itor.ViewTransform.parent != this.ViewTransform)
                    itor.ViewTransform.SetParent(this.ViewTransform);
                itor.ViewTransform.SetSiblingIndex(index);

                itor = itor.m_Next;
                index++;
            } while (itor != null);
            
            //3. update the final next
            BaseView nextView = m_Childs.Count > index ? m_Childs[index] : null;
            if (nextView != null)
            {
                nextView.m_Previous = m_Childs[index - 1];
                m_Childs[index - 1].m_Next = nextView;
            }
        }

        /// <summary>
        /// Remove child view from this view
        /// Iterate through all the next views, and remove them as well
        /// </summary>
        public void RemoveChild(BaseView childView)
        {
            if (!m_Childs.Contains(childView))
                return;

            //1. update previous
            BaseView preView = childView.m_Previous;
            if (preView != null)
            {
                preView.m_Next = null;
                childView.m_Previous = null;
            }

            //2. remove iteratively
            BaseView itor = childView;
            do
            {
                m_Childs.Remove(itor);
                itor.m_Parent = null;
                itor = itor.m_Next;
            } while (itor != null);
        }
        
        /// <summary>
        /// Set the next view of this view
        /// </summary>
        public void SetNext(BaseView nextView)
        {
            if (m_Next == nextView) return;

            if (nextView != null)
            {
                if (m_Parent != null)
                {
                    m_Parent.AddChild(nextView, SiblingIndex + 1);
                }
                else
                {
                    if (m_Next != null)
                    {
                        m_Next.m_Previous = nextView;
                        nextView.m_Next = m_Next;
                    }
                    m_Next = nextView;
                    nextView.m_Previous = this;
                }
            }
            else
            {
                m_Next.SetPrevious(null);
            }
        }

        /// <summary>
        /// Set the previous view of this view
        /// </summary>
        public void SetPrevious(BaseView preView)
        {
            if (m_Previous == preView) return;
            
            if (preView != null)
            {
                preView.SetNext(this);
            }
            else
            {
                //set null
                if (m_Parent != null)
                {
                    //remove from parent
                    m_Parent.RemoveChild(this);
                }
                else
                {
                    if (m_Previous != null)
                        m_Previous.m_Next = null;
                    m_Previous = null;
                }
            }
        }

        /// <summary>
        /// Get the topmost first child int the hierachy of this view
        /// </summary>
        /// <param name="untilConnection">stop util connection view. no need to count connection's child block view</param>
        public BaseView GetTopmostChild(bool untilConnection = true)
        {
            BaseView curView = this;
            while (curView.HasChild() && curView.m_Childs[0].Type != ViewType.Block)
            {
                curView = curView.m_Childs[0];
            }
            return curView;
        }

        /// <summary>
        /// Get the header view of this view
        /// </summary>
        public BaseView GetHeader()
        {
            BaseView header = this;
            while (header.m_Previous != null)
            {
                header = header.m_Previous;
            }
            return header;
        }

        /// <summary>
        /// Get the tail view of this view
        /// </summary>
        public BaseView GetTail()
        {
            BaseView tail = this;
            while (tail.m_Next != null)
            {
                tail = tail.m_Next;
            }
            return tail;
        }

        /// <summary>
        /// the local anchoredPosition of the view
        /// anchor: top-left, pivot: top-left
        /// </summary>
        public Vector2 XY
        {
            get { return m_ViewTransform.anchoredPosition; }
            set
            {
                if (m_ViewTransform.anchoredPosition != value)
                {
                    m_ViewTransform.anchoredPosition = value;
                    OnXYUpdated();
                }
            }
        }

        /// <summary>
        /// the local anchoredPosition for the center of the view
        /// anchor: top-left, pivot: top-left
        /// </summary>
        public Vector2 CenterXY
        {
            get { return XY + 0.5f * new Vector2(Width, -Height); }
            set { XY = value - 0.5f * new Vector2(Width, -Height); }
        }

        public Vector2 Size
        {
            get { return m_ViewTransform.rect.size; }
            set
            {
                bool changed = false;
                if (!Mathf.Approximately(m_ViewTransform.rect.width, value.x))
                {
                    changed = true;
                    m_ViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                }
                if (!Mathf.Approximately(m_ViewTransform.rect.height, value.y))
                {
                    changed = true;
                    m_ViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                }
                if (changed)
                    OnSizeUpdated();
            }
        }

        public float Width
        {
            get { return m_ViewTransform.rect.width; }
            set
            {
                if (!Mathf.Approximately(m_ViewTransform.rect.width, value))
                {
                    m_ViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
                    if (Application.isPlaying)
                        OnSizeUpdated();
                }
            }
        }

        public float Height
        {
            get { return m_ViewTransform.rect.height; }
            set
            {
                if (!Mathf.Approximately(m_ViewTransform.rect.height, value))
                {
                    m_ViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
                    if (Application.isPlaying)
                        OnSizeUpdated();
                }
            }
        }

        /// <summary>
        /// The position of the first child
        /// Subclass can override this to apply margin
        /// </summary>
        public virtual Vector2 ChildStartXY
        {
            get { return Vector2.zero; }
        }

        /// <summary>
        /// the position of the header view (the first child of its parent)
        /// </summary>
        public Vector2 HeaderXY
        {
            get { return m_Parent != null ? m_Parent.ChildStartXY : Vector2.zero; }
        }

        /// <summary>
        /// update layout of this view and all views effected
        /// </summary>
        public void UpdateLayout(Vector2 startPos)
        {
            Vector2 newSize = CalculateSize();
            bool changePos = XY != startPos;
            bool changeSize = Size != newSize;
            
            if (changePos) XY = startPos;
            if (changeSize) Size = newSize;

            switch (Type)
            {
                case ViewType.Field:
                case ViewType.Input:
                case ViewType.ConnectionInput:
                case ViewType.LineGroup:
                {
                    if (m_Next == null /*|| (!changePos && !changeSize)*/)
                    {
                        //reach the last child, or no change in current hierarchy, update it's parent view
                        m_Parent.UpdateLayout(m_Parent.SiblingIndex == 0 ? m_Parent.HeaderXY : m_Parent.XY);
                    }
                    else
                    {
                        //update next
                        if (Type != ViewType.LineGroup)
                        {
                            // same line
                            startPos.x += Size.x + BlockViewSettings.Get().ContentSpace.x;
                        }
                        else
                        {
                            // start a new line
                            startPos.y -= Size.y + BlockViewSettings.Get().ContentSpace.y;
                        }

                        BaseView topmostChild = m_Next.GetTopmostChild();
                        if (topmostChild != m_Next)
                        {
                            //need to update from its topmost child
                            m_Next.XY = startPos;
                            topmostChild.UpdateLayout(topmostChild.HeaderXY);
                        }
                        else
                        {
                            m_Next.UpdateLayout(startPos);
                        }
                    }
                    break;
                }
                case ViewType.Connection:
                case ViewType.Block:
                {
                    //no need to update its m_Next, as it is handled by Unity's Transform autolayout 
                    //update its parent directly
                    if (m_Parent != null)
                    {
                        m_Parent.UpdateLayout(m_Parent.SiblingIndex == 0 ? m_Parent.HeaderXY : m_Parent.XY);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// the type of this view
        /// </summary>
        public abstract ViewType Type { get; }

        /// <summary>
        /// calculate the size of this view
        /// </summary>
        protected abstract Vector2 CalculateSize();
        
        /// <summary>
        /// called when position is changed
        /// </summary>
        protected internal virtual void OnXYUpdated(){}
        
        /// <summary>
        /// called when size is updated
        /// </summary>
        protected internal virtual void OnSizeUpdated(){}
        
        public virtual void InitComponents()
        {
            m_ViewTransform = GetComponent<RectTransform>();
        }

        void Awake()
        {
            InitComponents();
        }

        void OnDestroy()
        {
            if (m_Parent != null)
                m_Parent.RemoveChild(this);
        }
    }
}
