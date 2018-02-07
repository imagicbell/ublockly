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
using System.IO;
using System.Linq;
using UBlockly;
using UnityEditor;
using UnityEngine;

namespace UBlockly.UGUI
{
    public class BlockViewEditor
    {
        [MenuItem("UBlockly/Build Block Prefabs")]
        static void BuildBlockPrefabs()
        {
            Blockly.Dispose();
            Blockly.Init();
            Workspace workspace = new Workspace();

            var blocks = BlockFactory.Instance.GetAllBlockDefinitions().Keys;
//            var blocks = BlockFactory.Instance.GetAllBlockDefinitions().Keys.Where(s => s.Equals("lists_create_with"));

            if (!Directory.Exists(BlockResMgr.Get().BlockViewPrefabPath))
                Directory.CreateDirectory(BlockResMgr.Get().BlockViewPrefabPath);
            
            BlockResMgr.Get().ClearBlockViewPrefabs();

            try
            {
                int index = 0;
                int count = blocks.Count();
                foreach (string name in blocks)
                {
                    EditorUtility.DisplayProgressBar(null, "Building block prefab: " + name, index / (float) count);

                    Block block = workspace.NewBlock(name);
                    GameObject obj = BlockViewBuilder.BuildBlockView(block);

                    string path = BlockResMgr.Get().BlockViewPrefabPath + obj.name + ".prefab";
                    GameObject prefab = PrefabUtility.CreatePrefab(path, obj, ReplacePrefabOptions.Default);
                    BlockResMgr.Get().AddBlockViewPrefab(prefab);

                    GameObject.DestroyImmediate(obj);

                    index++;
                }
            }
            finally
            {
                AssetDatabase.SaveAssets();
                Resources.UnloadUnusedAssets();
                
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
