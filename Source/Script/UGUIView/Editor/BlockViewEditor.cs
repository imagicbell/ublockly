using System;
using System.IO;
using System.Linq;
using PTGame.Blockly;
using UnityEditor;
using UnityEngine;

namespace PTGame.Blockly.UGUI
{
    public class BlockViewEditor
    {
        [MenuItem("PTBlockly/Build Block Prefabs")]
        static void BuildBlockPrefabs()
        {
            Blockly.Dispose();
            Blockly.LoadAllBlocksFromJson();
            Workspace workspace = new Workspace();

            var blocks = BlockFactory.Instance.GetAllBlockDefinitions().Keys.Where(s => !s.StartsWith("colour"));
            //var blocks = BlockFactory.Instance.GetAllBlockDefinitions().Keys.Where(s => s.Equals("lists_create_with"));

            BlockViewFactory viewFactory = ScriptableObject.CreateInstance<BlockViewFactory>();
            string factoryPath = AssetDatabase.GetAssetPath(BlockViewFactory.Get());

            if (!Directory.Exists(BlockViewSettings.Get().BlockPrefabPath))
                Directory.CreateDirectory(BlockViewSettings.Get().BlockPrefabPath);

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
                    viewFactory.AddPrefab(prefab);

                    GameObject.DestroyImmediate(obj);

                    index++;
                }
            }
            finally
            {
                AssetDatabase.CreateAsset(viewFactory, factoryPath);
                
                AssetDatabase.SaveAssets();
                Resources.UnloadUnusedAssets();
                
                EditorUtility.ClearProgressBar();
            }
        }
    }
}