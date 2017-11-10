using System;
using System.Linq;
using UnityEngine;

namespace UBlockly.UGUI
{
    public static class DialogFactory
    {
        public static BaseDialog CreateDialog(string dialogId, Transform parent = null)
        {
            GameObject prefab = BlockResMgr.Get().LoadDialogPrefab(dialogId);
            if (prefab == null)
                throw new Exception("Can\'t find dialog prefab for " + dialogId + ", Please ensure you configure it in \"BlockResSettings\"");

            if (parent == null)
                parent = BlocklyUI.UICanvas.transform;
            
            GameObject dialogObj = GameObject.Instantiate(prefab, parent, false);
            BaseDialog dialog = dialogObj.GetComponent<BaseDialog>();
            dialog.Init();
            return dialog;
        }
        
        public static BaseDialog CreateMutatorDialog(Block block, Transform parent = null)
        {
            if (block.Mutator == null)
                return null;
            
            GameObject prefab = BlockResMgr.Get().LoadDialogPrefab(block.Mutator.MutatorId);
            if (prefab == null)
                throw new Exception("Can\'t find dialog prefab for " + block.Mutator.MutatorId + ", Please ensure you configure it in \"BlockResSettings\"");
            
            if (parent == null)
                parent = BlocklyUI.UICanvas.transform;
            
            GameObject dialogObj = GameObject.Instantiate(prefab, parent, false);
            BaseDialog dialog = dialogObj.GetComponent<BaseDialog>();
            dialog.Init(block);
            return dialog;
        }
    }
}
