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

            var blocks = BlockFactory.Instance.GetAllBlockDefinitions().Keys.Where(s => !s.StartsWith("colour"));
            //var blocks = BlockFactory.Instance.GetAllBlockDefinitions().Keys.Where(s => s.Equals("lists_create_with"));

            if (!Directory.Exists(BlockViewSettings.Get().BlockPrefabPath))
                Directory.CreateDirectory(BlockViewSettings.Get().BlockPrefabPath);
            
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

                    string path = BlockViewSettings.Get().BlockPrefabPath + obj.name + ".prefab";
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
