using System;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class BaseDialog : MonoBehaviour
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

        private void OnDestroy()
        {
            if (mOnCloseEvent != null)
            {
                mOnCloseEvent.Invoke();
                mOnCloseEvent = null;
            }
        }

        protected virtual void OnInit(){}
    }
}
