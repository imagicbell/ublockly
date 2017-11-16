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

        public static BaseDialog CreateDialog<T>(string dialogId, Transform parent = null) where T : BaseDialog
        {
            return CreateDialog(dialogId, parent) as T;
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

        public static T CreateMutatorDialog<T>(Block block, Transform parent = null) where T : BaseDialog
        {
            return CreateMutatorDialog(block, parent) as T;
        }

        public static FieldDialog CreateFieldDialog(Field field, Transform parent = null)
        {
            GameObject prefab = BlockResMgr.Get().LoadDialogPrefab(field.Type);
            if (prefab == null)
                throw new Exception("Can\'t find dialog prefab for " + field.Type + ", Please ensure you configure it in \"BlockResSettings\"");
            
            if (parent == null)
                parent = BlocklyUI.UICanvas.transform;
            
            GameObject dialogObj = GameObject.Instantiate(prefab, parent, false);
            FieldDialog dialog = dialogObj.GetComponent<FieldDialog>();
            dialog.Init(field);
            return dialog;
        }

        public static T CreateFieldDialog<T>(Field field, Transform parent = null) where T : FieldDialog
        {
            return CreateFieldDialog(field, parent) as T;
        }
    }
}
