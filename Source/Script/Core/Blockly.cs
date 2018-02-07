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
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    public class Blockly
    {
        /// <summary>
        /// Initialize blockly model. Called very first when start blockly
        /// </summary>
        public static void Init()
        {
            BlockResMgr.Get().LoadI18n(I18n.EN);
            BlockResMgr.Get().LoadJsonDefinitions();
        }
        
        /// <summary>
        /// clear all blocks loaded
        /// </summary>
        public static void Dispose()
        {
            BlockFactory.Instance.Clear();
            I18n.Dispose();
        }
        
        /// <summary>
        /// Define blocks from an array of JSON block definitions, as might be generated
        /// by the Blockly Developer Tools.
        /// </summary>
        /// <param name="jsonArray"></param>
        [Obsolete("Only used for test case. Use BlockFactory.Instance.AddJsonDefinitions instead.")]
        public static void DefineBlocksWithJsonArray(JArray jsonArray)
        {
            BlockFactory.Instance.AddJsonDefinitions(jsonArray.ToString());
        }
    }
}
