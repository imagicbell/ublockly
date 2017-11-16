using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public abstract class BaseDialog : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Button m_ButtonOK;
        
        protected Block mBlock;
        public Block Block { get { return mBlock; } }

        private event Action mOnCloseEvent;
        
        /// <summary>
        /// Intialization called after gameobject created
        /// </summary>
        public void Init()
        {
            OnInit();
            
            m_ButtonOK.onClick.AddListener(() =>
            {
                if (mOnCloseEvent != null)
                {
                    mOnCloseEvent.Invoke();
                    mOnCloseEvent = null;
                }
                
                GameObject.Destroy(this.gameObject);
            });
        }

        /// <summary>
        /// Intialization called after gameobject created
        /// </summary>
        public void Init(Block block)
        {
            mBlock = block;
            Init();
        }

        /// <summary>
        /// Add listeners for destroying the dialog
        /// </summary>
        public void AddCloseEvent(Action onClose)
        {
            mOnCloseEvent += onClose;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            GameObject.Destroy(this.gameObject);
        }

        protected virtual void OnInit(){}
    }
}
