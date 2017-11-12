using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public enum BlockResLoadType
    {
        /// <summary>
        /// Serialized in scriptable object
        /// </summary>
        Serialized = 1,
        /// <summary>
        /// Load from Resources 
        /// </summary>
        Resources = 2,
        /// <summary>
        /// Load from Assetbundle
        /// </summary>
        Assetbundle = 3,
    }
    
    [Serializable]
    public class BlockResParam
    {
        public string IndexName;
        public string ResName;
    }

    [Serializable]
    public class BlockObjectParam : BlockResParam
    {
        public GameObject Prefab;
    }
    
    [Serializable]
    public class BlockTextResParam : BlockResParam
    {
        public TextAsset TextFile;
    }
    
    /// <summary>
    /// delegate for loading asset from Assetbundle 
    /// </summary>
    public delegate T AssetbundleLoad<T>(string assetName) where T : UnityEngine.Object;
    
    /// <summary>
    /// manage all resources. 
    /// This can be customized according to resources management in each project 
    /// </summary>
    [CreateAssetMenu(menuName = "PTBlockly/BlockResSettings", fileName = "BlockResSettings")]
    public class BlockResMgr : ScriptableObject
    {
        [SerializeField] private BlockResLoadType m_LoadType;
        [SerializeField] private List<BlockTextResParam> m_I18nFiles;
        [SerializeField] private List<BlockTextResParam> m_BlockJsonFiles;
        [SerializeField] private List<BlockObjectParam> m_BlockViewPrefabs;
        [SerializeField] private List<BlockObjectParam> m_DialogPrefabs;

        public BlockResLoadType LoadType
        {
            get { return m_LoadType; }
        }

        private AssetbundleLoad<UnityEngine.Object> mAssetbundleLoad;

        public void SetAssetbundleLoadDelegate(AssetbundleLoad<UnityEngine.Object> del)
        {
            mAssetbundleLoad = del;
        }

        #region I18n Files

        public void LoadI18n(string language)
        {
            if (m_I18nFiles == null || m_I18nFiles.Count == 0)
            {
                Debug.LogError("LoadI18n failed. Please assign i18n files to BlockResSettings.asset.");
                return;
            }

            TextAsset textAsset = null;
            foreach (BlockTextResParam resParam in m_I18nFiles)
            {
                if (language.Equals(resParam.IndexName))
                {
                    switch (m_LoadType)
                    {
                        case BlockResLoadType.Assetbundle:
                            if (mAssetbundleLoad != null)
                                textAsset = mAssetbundleLoad(resParam.ResName) as TextAsset;
                            break;
                        case BlockResLoadType.Resources:
                            textAsset = Resources.Load<TextAsset>(resParam.ResName);
                            break;
                        case BlockResLoadType.Serialized:
                            textAsset = resParam.TextFile;
                            break;
                    }
                }
            }
            if (textAsset != null)
                I18n.Init(textAsset.text);
        }

        #endregion

        #region Block Json Files
        
        public void LoadJsonDefinitions()
        {
            if (m_BlockJsonFiles == null || m_BlockJsonFiles.Count == 0)
                return;

            TextAsset textAsset = null;
            foreach (BlockTextResParam resParam in m_BlockJsonFiles)
            {
                switch (m_LoadType)
                {
                    case BlockResLoadType.Assetbundle:
                        if (mAssetbundleLoad != null)
                            textAsset = mAssetbundleLoad(resParam.ResName) as TextAsset;
                        break;
                    case BlockResLoadType.Resources:
                        textAsset = Resources.Load<TextAsset>(resParam.ResName);
                        break;
                    case BlockResLoadType.Serialized:
                        textAsset = resParam.TextFile;
                        break;
                }

                if (textAsset != null)
                    BlockFactory.Instance.AddJsonDefinitions(JArray.Parse(textAsset.text));
            }
        }

        #endregion

        #region Block View Prefabs
        
        public GameObject LoadBlockViewPrefab(string blockType)
        {
            if (m_BlockViewPrefabs == null || m_BlockViewPrefabs.Count == 0)
                return null;

            BlockObjectParam resParam = m_BlockViewPrefabs.Find(o => o.IndexName.Equals(blockType));
            if (resParam == null)
                return null;

            GameObject blockPrefab = null;
            switch (m_LoadType)
            {
                case BlockResLoadType.Assetbundle:
                    if (mAssetbundleLoad != null)
                        blockPrefab = mAssetbundleLoad(resParam.ResName) as GameObject;
                    break;
                case BlockResLoadType.Resources:
                    blockPrefab = Resources.Load<GameObject>(resParam.ResName);
                    break;
                case BlockResLoadType.Serialized:
                    blockPrefab = resParam.Prefab;
                    break;
            }
            return blockPrefab;
        }

        public void AddBlockViewPrefab(GameObject blockPrefab)
        {
            if (m_BlockViewPrefabs == null)
                m_BlockViewPrefabs = new List<BlockObjectParam>();

            string prefabName = blockPrefab.name.Replace("(Clone)", "");
            string indexName = prefabName.Substring("Block_".Length);
            if (m_BlockViewPrefabs.Exists(o => o.IndexName.Equals(indexName)))
                return;

            BlockObjectParam resParam = new BlockObjectParam();
            resParam.IndexName = indexName;
            resParam.ResName = prefabName;
            if (m_LoadType == BlockResLoadType.Serialized)
                resParam.Prefab = blockPrefab;
            m_BlockViewPrefabs.Add(resParam);
        }

        public void ClearBlockViewPrefabs()
        {
            m_BlockViewPrefabs.Clear();
        }
        
        #endregion
        
        #region Dialog Prefabs

        public GameObject LoadDialogPrefab(string dialogId)
        {
            if (m_DialogPrefabs == null || m_DialogPrefabs.Count == 0)
                return null;

            BlockObjectParam resParam = m_DialogPrefabs.Find(o => o.IndexName.Equals(dialogId));
            if (resParam == null)
                return null;
            
            GameObject dialogPrefab = null;
            switch (m_LoadType)
            {
                case BlockResLoadType.Assetbundle:
                    if (mAssetbundleLoad != null)
                        dialogPrefab = mAssetbundleLoad(resParam.ResName) as GameObject;
                    break;
                case BlockResLoadType.Resources:
                    dialogPrefab = Resources.Load<GameObject>(resParam.ResName);
                    break;
                case BlockResLoadType.Serialized:
                    dialogPrefab = resParam.Prefab;
                    break;
            }
            return dialogPrefab;
        }
        
        #endregion

        private static BlockResMgr mInstance = null; 
        public static BlockResMgr Get()
        {
            if (mInstance == null)
                mInstance = Resources.Load<BlockResMgr>("BlockResSettings");
            if (mInstance == null)
                throw new Exception("There is no \"BlockResSettings\" ScriptObject under Resources folder");
                
            return mInstance;
        }

        public static void Dispose()
        {
            mInstance = null;
        }
    }
}
