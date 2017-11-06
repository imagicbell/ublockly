using System.Collections.Generic;
using UnityEngine;

namespace PTGame.Blockly
{
    public static class Variables
    {
        /// <summary>
        /// Constant to seperate variable names from procedures and generated functions
        /// When running generators
        /// </summary>
        public const string NAME_TYPE = Blockly.VARIABLE_CATEGORY_NAME;

        /// <summary>
        /// Find all user-created variables that are in use in the workspace.
        /// For use by generators.
        /// </summary>
        /// <param name="root"> Root block or workspace</param>
        /// <returns></returns>
        public static List<string> AllUsedVariables(Workspace root)
        {
            // Root is Workspace
            return GetUsedVariablesFromBlocks(root.GetAllBlocks());
        }
        
        /// <summary>
        /// Find all user-created variables that are in use in the workspace.
        /// For use by generators.
        /// </summary>
        /// <param name="root"> Root block or workspace</param>
        /// <returns></returns>
        public static List<string> AllUsedVariables(Block root)
        {
            return GetUsedVariablesFromBlocks(root.GetDescendants());
        }

        private static List<string> GetUsedVariablesFromBlocks(List<Block> blocks)
        {
            var variableHash = new Dictionary<string,string>();
            // Iterate through every block and add each variable to the hash.
            foreach (var block in blocks)
            {
                var blockVariables = block.GetVars();
                if (null != blockVariables && blockVariables.Count != 0)
                {
                    foreach (var varName in blockVariables)
                    {
                        // Variable name may be null if the block is only half-built.
                        if (!string.IsNullOrEmpty(varName))
                        {
                            if (!variableHash.ContainsKey(varName.ToLower()))
                            {
                                variableHash.Add(varName.ToLower(), varName);
                            }
                            else
                            {
                                variableHash[varName.ToLower()] = varName;
                            }
                        }
                    }
                }
            }
            // Flatten the hash into a list.
            return new List<string>(variableHash.Values);
        }
    }
}