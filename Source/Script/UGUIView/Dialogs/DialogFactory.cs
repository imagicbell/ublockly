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
