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
