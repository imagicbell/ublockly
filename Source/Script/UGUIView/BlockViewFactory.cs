using System;
using System.Collections.Generic;
using UnityEngine;

namespace PTGame.Blockly.UGUI
{
    /// <summary>
    /// factory to create block view
    /// developers can change this to static class and customnly load prefabs from Assetbundles or Resources. 
    /// </summary>
    [CreateAssetMenu(menuName = "PTBlockly/BlockViewFactory", fileName = "BlockViewFactory")]
    public class BlockViewFactory : ScriptableObject
    {
        /// <summary>
        /// a list of block view prototype gameobject
        /// </summary>
        [SerializeField] private List<GameObject> m_BlockPrefabs;

        /// <summary>
        /// created at runtime
        /// </summary>
        private Dictionary<string, GameObject> mBlockPrefabNotSerialized = new Dictionary<string, GameObject>();

        public void AddPrefab(GameObject blockPrefab)
        {
            if (m_BlockPrefabs == null)
                m_BlockPrefabs = new List<GameObject>();
            
            if (!m_BlockPrefabs.Contains(blockPrefab))
                m_BlockPrefabs.Add(blockPrefab);
        }

        public BlockView CreateView(Block block)
        {
            BlockView blockView;

            GameObject blockPrefab = m_BlockPrefabs.Find(b => b.name.Equals("Block_" + block.Type));
            if (blockPrefab == null)
                mBlockPrefabNotSerialized.TryGetValue(block.Type, out blockPrefab);
            
            if (blockPrefab != null)
            {
                // has been builded beforehand
                GameObject blockObj = GameObject.Instantiate(blockPrefab);
                blockObj.name = blockPrefab.name;
                
                blockView = blockObj.GetComponent<BlockView>();
                blockView.BindModel(block);
                
                // rebuild inputs for mutation blocks
                if (block.Mutator != null)
                    BlockViewBuilder.BuildInputViews(block, blockView);
                
                blockView.BuildLayout();
            }
            else
            {
                blockPrefab = BlockViewBuilder.BuildBlockView(block);
                mBlockPrefabNotSerialized.Add(block.Type, blockPrefab);
                
                blockView = blockPrefab.GetComponent<BlockView>();
                blockView.BindModel(block);
                
                // BlockViewBuilder.BuildBlockView will do both "BuildInputViews" and "BuildLayout"
            }
            return blockView;
        }
        
        private static BlockViewFactory mInstance = null; 
        public static BlockViewFactory Get()
        {
            if (mInstance == null)
                mInstance = Resources.Load<BlockViewFactory>("BlockViewFactory");
            if (mInstance == null)
                throw new Exception("There is no \"BlockViewFactory\" ScriptObject under Resources folder");
                
            return mInstance;
        }

        public static void Dispose()
        {
            if (mInstance != null)
            {
                mInstance.mBlockPrefabNotSerialized.Clear();
                mInstance = null;   
            }
        }
    }
}