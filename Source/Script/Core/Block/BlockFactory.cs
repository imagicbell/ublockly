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

        private BlockFactory(){}
        
        private Dictionary<string, BlockDefinition> mDefinitions = new Dictionary<string, BlockDefinition>();

        public Dictionary<string, BlockDefinition> GetAllBlockDefinitions()
        {
            return mDefinitions;
        }

        private Dictionary<string, List<string>> mCategories = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> GetCategories()
        {
            return mCategories;
        }

        /// <summary>
        /// get the category name for block type
        /// </summary>
        public string GetCategoryOfBlockType(string blockType)
        {
            foreach (string category in mCategories.Keys)
            {
                if (mCategories[category].Contains(blockType))
                    return category;
            }
            return null;
        }

        /// <summary>
        /// remove all containers in the factory
        /// </summary>
        public void Clear()
        {
            mDefinitions.Clear();
        }

        /// <summary>
        /// Loads and adds block definitions from a JSON array in an input stream.
        /// </summary>
        /// <param name="jsonArray"></param>
        public void AddJsonDefinitions(JArray jsonArray)
        {
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
                string categoryName = length > 0 ? typeName.Substring(0, length) : typeName;
                if (!mCategories.ContainsKey(categoryName))
                    mCategories[categoryName] = new List<string>();
                mCategories[categoryName].Add(typeName);
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
                int colorHue = definition.GetHueColor();
                
                block.Reshape(inputs, output, prev, next);
                block.ColorHue = colorHue;

                if (mutator != null) block.SetMutator(mutator);
                if (inputsInline) block.SetInputsInline(true);
            }
            return block;
        }
    }
}
