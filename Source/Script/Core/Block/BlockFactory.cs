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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    /// <summary>
    /// collection of block definitions, mutators, block extensions
    /// factory to create a block
    /// </summary>
    public class BlockFactory
    {
        private static BlockFactory mInstance = null;
        public static BlockFactory Instance
        {
            get { return mInstance ?? (mInstance = new BlockFactory()); }
        }

        private Dictionary<string, BlockDefinition> mDefinitions = new Dictionary<string, BlockDefinition>();

        public Dictionary<string, BlockDefinition> GetAllBlockDefinitions()
        {
            return mDefinitions;
        }

        private Dictionary<string, List<string>> mPrefixCategories = new Dictionary<string, List<string>>();

        /// <summary>
        /// Get block types width the specific prefix
        /// </summary>
        public List<string> GetBlockTypesOfPrefix(string prefix)
        {
            List<string> blockTypes;
            mPrefixCategories.TryGetValue(prefix, out blockTypes);
            return blockTypes;
        }

        /// <summary>
        /// remove all containers in the factory
        /// </summary>
        public void Clear()
        {
            mDefinitions.Clear();
            mPrefixCategories.Clear();
        }

        /// <summary>
        /// Loads and adds block definitions from a JSON array in an input stream.
        /// </summary>
        public void AddJsonDefinitions(string jsonText)
        {
            JArray jsonArray = JArray.Parse(jsonText);
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JObject element = jsonArray[i] as JObject;
                string typeName = element["type"].ToString();
                if (mDefinitions.ContainsKey(typeName))
                {
                    Debug.LogError("Block definition in JSON array has duplicated type name in prior definition of " + typeName);
                }
                else
                {
                    mDefinitions[typeName] = new BlockDefinition(element);
                }

                int length = typeName.IndexOf("_");
                string prefix = length > 0 ? typeName.Substring(0, length) : typeName;
                if (!mPrefixCategories.ContainsKey(prefix))
                    mPrefixCategories[prefix] = new List<string>();
                mPrefixCategories[prefix].Add(typeName);
            }
        }

        /// <summary>
        /// Create a block from block definitions
        /// </summary>
        /// <param name="workspace">The Block's workspace</param>
        /// <param name="type">block unique type</param>
        /// <param name="uid">block uid</param>
        /// <returns></returns>
        public Block CreateBlock(Workspace workspace, string type, string uid = null)
        {
            Block block = new Block(workspace, type, uid);
            
            BlockDefinition definition;
            if (!mDefinitions.TryGetValue(type, out definition))
            {
                //Debug.LogWarning("There is no block definition for type: " + type + ". Please ensure to load it first");
                Debug.Log("Create an empty block: " + type);
            }
            else
            {
                List<Input> inputs = definition.CreateInputList();
                Connection output = definition.CreateOutputConnection();
                Connection prev = definition.CreatePreviousStatementConnection();
                Connection next = definition.CreateNextStatementConnection();
                Mutator mutator = definition.CreateMutator();
                bool inputsInline = definition.GetInputsInlineDefault();
                
                block.Reshape(inputs, output, prev, next);

                if (mutator != null) block.SetMutator(mutator);
                if (inputsInline) block.SetInputsInline(true);
            }
            return block;
        }
    }
}
