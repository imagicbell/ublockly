using System;
using System.Linq;
using UnityEngine;

namespace PTGame.Blockly.UGUI
{
    [CreateAssetMenu(menuName = "PTBlockly/DialogFactory", fileName = "DialogFactory")]
    public class DialogFactory : ScriptableObject
    {
        [Serializable]
        public class PrefabInfo
        {
            public string Id;
            public GameObject Prefab;
        }

        [SerializeField] private PrefabInfo[] m_Dialogs;
        [SerializeField] private PrefabInfo[] m_MutatorDialogs;

        public BaseDialog CreateDialog(string dialogId, Transform parent = null)
        {
            if (m_Dialogs == null)
                return null;

            if (parent == null)
                parent = BlocklyUI.UICanvas.transform;
            
            foreach (PrefabInfo info in m_Dialogs)
            {
                if (info.Id.Equals(dialogId))
                {
                    GameObject dialogObj = GameObject.Instantiate(info.Prefab, parent, false);
                    BaseDialog dialog = dialogObj.GetComponent<BaseDialog>();
                    dialog.Init();
                    return dialog;
                }       
            }
            return null;
        }
        
        public BaseDialog CreateMutatorDialog(Block block, Transform parent = null)
        {
            if (m_MutatorDialogs == null)
                return null;
            if (block.Mutator == null)
                return null;
            
            if (parent == null)
                parent = BlocklyUI.UICanvas.transform;

            foreach (PrefabInfo info in m_MutatorDialogs)
            {
                if (info.Id.Equals(block.Mutator.MutatorId))
                {
                    GameObject dialogObj = GameObject.Instantiate(info.Prefab, parent, false);
                    BaseDialog dialog = dialogObj.GetComponent<BaseDialog>();
                    dialog.Init(block);
                    return dialog;
                }       
            }
            return null;
        }
        
        private static DialogFactory mInstance = null; 
        public static DialogFactory Get()
        {
            if (mInstance == null)
                mInstance = Resources.Load<DialogFactory>("DialogFactory");
            if (mInstance == null)
                throw new Exception("There is no \"DialogFactory\" ScriptObject under Resources folder");
                
            return mInstance;
        }

        public static void Dispose()
        {
            mInstance = null;
        }
    }
}