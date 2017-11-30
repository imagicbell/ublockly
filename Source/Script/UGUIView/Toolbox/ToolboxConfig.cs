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

        public static ToolboxConfig Load(string configName)
        {
            ToolboxConfig config = BlockResMgr.Get().LoadToolboxConfig(configName);
            if (config == null)
                throw new Exception("Can\'t load ToolboxConfig: " + configName);
            
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
        
        public Color Color { get; private set; }

        public void Init()
        {
            if (mInited) return;

            if (!string.IsNullOrEmpty(BlockTypePrefix))
            {
                BlockList.AddRange(BlockFactory.Instance.GetBlockTypesOfPrefix(BlockTypePrefix));
            }

            Color color;
            ColorUtility.TryParseHtmlString(ColorHex, out color);
            Color = color;
        }
    }
}