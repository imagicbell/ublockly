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
using System.Collections.Generic;
using UnityEngine;

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
    /// manage all resources. 
    /// This can be customized according to resources management in each project 
    /// </summary>
    [CreateAssetMenu(menuName = "UBlockly/BlockResSettings", fileName = "BlockResSettings")]
    public class BlockResMgr : ScriptableObject
    {
        [SerializeField] private BlockResLoadType m_LoadType;
        [SerializeField] private List<BlockTextResParam> m_I18nFiles;
        [SerializeField] private List<BlockTextResParam> m_BlockJsonFiles;
        [SerializeField] private List<BlockTextResParam> m_ToolboxFiles;

        [SerializeField] public string m_BlockViewPrefabPath;
        [SerializeField] private List<BlockObjectParam> m_BlockViewPrefabs;
        
        [SerializeField] private List<BlockObjectParam> m_DialogPrefabs;

        public BlockResLoadType LoadType
        {
            get { return m_LoadType; }
        }

        public string BlockViewPrefabPath
        {
            get { return m_BlockViewPrefabPath; }
        }

        private Func<string, UnityEngine.Object> mABSyncLoad;
        private Action<string, Action<UnityEngine.Object>> mABASyncLoad;
        private Action<string> mABUnload;

        /// <summary>
        /// Set the delegate for synchronously loading object from assetbundles
        /// </summary>
        public void SetAssetbundleSyncLoadDelegate(Func<string, UnityEngine.Object> del)
        {
            mABSyncLoad = del;
        }

        /// <summary>
        /// Set the delegate for asynchronously loading object from assetbundles
        /// </summary>
        public void SetAssetbundleASyncLoadDelegate(Action<string, Action<UnityEngine.Object>> del)
        {
            mABASyncLoad = del;
        }

        /// <summary>
        /// Set the delegate for unloading object from assetbundle
        /// </summary>
        public void SetAssetbundleUnloadDelegate(Action<string> del)
        {
            mABUnload = del;
        }

        #region I18n Files

        public void LoadI18n(string language)
        {
            if (m_I18nFiles == null || m_I18nFiles.Count == 0)
            {
                Debug.LogError("LoadI18n failed. Please assign i18n files to BlockResSettings.asset.");
                return;
            }

            foreach (BlockTextResParam resParam in m_I18nFiles)
            {
                if (language.Equals(resParam.IndexName))
                {
                    TextAsset textAsset = null;
                    switch (m_LoadType)
                    {
                        case BlockResLoadType.Assetbundle:
                            if (mABSyncLoad != null)
                                textAsset = mABSyncLoad(resParam.ResName) as TextAsset;
                            break;
                        case BlockResLoadType.Resources:
                            textAsset = Resources.Load<TextAsset>(resParam.ResName);
                            break;
                        case BlockResLoadType.Serialized:
                            textAsset = resParam.TextFile;
                            break;
                    }
                    if (textAsset != null)
                    {
                        I18n.AddI18nFile(textAsset.text);
                        if (m_LoadType == BlockResLoadType.Assetbundle && mABUnload != null)
                            mABUnload(resParam.ResName);
                    }
                }
            }
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
                        if (mABSyncLoad != null)
                            textAsset = mABSyncLoad(resParam.ResName) as TextAsset;
                        break;
                    case BlockResLoadType.Resources:
                        textAsset = Resources.Load<TextAsset>(resParam.ResName);
                        break;
                    case BlockResLoadType.Serialized:
                        textAsset = resParam.TextFile;
                        break;
                }

                if (textAsset != null)
                {
                    BlockFactory.Instance.AddJsonDefinitions(textAsset.text);
                    if (m_LoadType == BlockResLoadType.Assetbundle && mABUnload != null)
                        mABUnload(resParam.ResName);
                }
            }
        }

        #endregion
        
        #region Toolbox Config Files

        public UGUI.ToolboxConfig LoadToolboxConfig(string configName)
        {
            if (m_ToolboxFiles == null || m_ToolboxFiles.Count == 0)
                return null;

            UGUI.ToolboxConfig toolboxConfig = null;
            TextAsset textAsset = null;
            foreach (BlockTextResParam resParam in m_ToolboxFiles)
            {
                if (string.Equals(configName, resParam.IndexName))
                {
                    switch (m_LoadType)
                    {
                        case BlockResLoadType.Assetbundle:
                            if (mABSyncLoad != null)
                                textAsset = mABSyncLoad(resParam.ResName) as TextAsset;
                            break;
                        case BlockResLoadType.Resources:
                            textAsset = Resources.Load<TextAsset>(resParam.ResName);
                            break;
                        case BlockResLoadType.Serialized:
                            textAsset = resParam.TextFile;
                            break;
                    }

                    if (textAsset != null)
                    {
                        toolboxConfig = JsonUtility.FromJson<UGUI.ToolboxConfig>(textAsset.text);
                        if (m_LoadType == BlockResLoadType.Assetbundle && mABUnload != null)
                            mABUnload(resParam.ResName);
                    }    
                    break;
                }
            }
            return toolboxConfig;
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
                    if (mABSyncLoad != null)
                        blockPrefab = mABSyncLoad(resParam.ResName) as GameObject;
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

        public void UnloadBlockViewPrefab(string blockType)
        {
            if (m_BlockViewPrefabs == null || m_BlockViewPrefabs.Count == 0)
                return;

            BlockObjectParam resParam = m_BlockViewPrefabs.Find(o => o.IndexName.Equals(blockType));
            if (resParam == null)
                return;

            if (m_LoadType == BlockResLoadType.Assetbundle && mABUnload != null)
                mABUnload(resParam.ResName);
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
                    if (mABSyncLoad != null)
                        dialogPrefab = mABSyncLoad(resParam.ResName) as GameObject;
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

        public void UnloadDialogPrefab(string dialogId)
        {
            if (m_DialogPrefabs == null || m_DialogPrefabs.Count == 0)
                return;

            BlockObjectParam resParam = m_DialogPrefabs.Find(o => o.IndexName.Equals(dialogId));
            if (resParam == null)
                return;

            if (m_LoadType == BlockResLoadType.Assetbundle && mABUnload != null)
                mABUnload(resParam.ResName);
        }

        #endregion

        public Texture2D LoadTexture(string texName)
        {
            if (mABSyncLoad != null)
                return mABSyncLoad(texName) as Texture2D;
            return Resources.Load<Texture2D>(texName);
        }

        public void UnloadTexture(string texName)
        {
            if (mABUnload != null)
                mABUnload(texName);
        }

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
