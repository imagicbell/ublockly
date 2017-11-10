using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    public abstract partial class Blockly
    {
        /// <summary>
        /// Define blocks from an array of JSON block definitions, as might be generated
        /// by the Blockly Developer Tools.
        /// </summary>
        /// <param name="jsonArray"></param>
        [Obsolete("Only used for test case. Use BlockFactory.Instance.AddJsonDefinitions instead.")]
        public static void DefineBlocksWithJsonArray(JArray jsonArray)
        {
            BlockFactory.Instance.AddJsonDefinitions(jsonArray);
        }

        /// <summary>
        /// load all blocks defined in json
        /// json files are configured in ScriptObject "JsonCollection"
        /// </summary>
        public static void LoadAllBlocksFromJson()
        {
            BlockResMgr.Get().LoadJsonDefinitions();
        }
        
        /// <summary>
        /// clear all blocks loaded
        /// </summary>
        public static void Dispose()
        {
            BlockFactory.Instance.Clear();
        }
    }
}
