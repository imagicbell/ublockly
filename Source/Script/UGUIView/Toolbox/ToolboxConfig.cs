using System;
using System.Collections.Generic;
using UnityEngine;

namespace UBlockly.UGUI
{
    [Serializable]
    public class ToolboxConfig
    {
        public string Style;
        public List<ToolboxBlockCategory> BlockCategoryList;

        public ToolboxBlockCategory GetBlockCategory(string categoryName)
        {
            var category = BlockCategoryList.Find(c => c.CategoryName.Equals(categoryName));
            if (category == null)
                throw new Exception(string.Format("Can\'t find category configuration for \"{0}\" in Toolbox json configuration.", categoryName));
            return category;
        }

        public static ToolboxConfig ParseFromJson(string jsonText)
        {
            ToolboxConfig config = JsonUtility.FromJson<ToolboxConfig>(jsonText);
            if (config == null)
                throw new Exception("Can\'t parse ToolboxConfig from json text:\n" + jsonText);
            
            foreach (var category in config.BlockCategoryList)
            {
                category.Init();
            }
            return config;
        }
    }

    [Serializable]
    public class ToolboxBlockCategory
    {
        public string CategoryName;
        public string ColorHex;
        public string BlockTypePrefix;
        public List<string> BlockList;


        [NonSerialized] private bool mInited = false;

        public void Init()
        {
            if (mInited) return;

            if (!string.IsNullOrEmpty(BlockTypePrefix))
            {
                BlockList.AddRange(BlockFactory.Instance.GetBlockTypesOfPrefix(BlockTypePrefix));
            }
        }
    }
}