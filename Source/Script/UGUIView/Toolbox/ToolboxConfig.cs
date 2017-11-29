using System;
using System.Collections.Generic;

namespace UBlockly.UGUI
{
    [Serializable]
    public class ToolboxConfig
    {
        public string Style;
        public ToolboxBlockCategory BlockCategory;
    }

    public class ToolboxBlockCategory
    {
        public string CategoryName;
        public List<string> BlockList;
    }
}