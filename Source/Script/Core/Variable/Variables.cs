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
using UnityEngine;

namespace UBlockly
{
    public static class Variables
    {
        /// <summary>
        /// Constant to seperate variable names from procedures and generated functions
        /// When running generators
        /// </summary>
        public const string NAME_TYPE = Define.VARIABLE_CATEGORY_NAME;

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
