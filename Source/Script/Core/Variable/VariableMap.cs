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
using System.Linq;

namespace UBlockly
{
    public struct VariableUpdateData
    {
        public const int Create = 1;
        public const int Delete = 2;
        public const int Rename = 3;

        public int Type;
        
        /// <summary>
        /// The variable name which the update is concerned 
        /// </summary>
        public string VarName;
        
        /// <summary>
        /// The new variable name for renaming.
        /// </summary>
        public string NewVarName;

        public VariableUpdateData(int type, string varName)
        {
            Type = type;
            VarName = varName;
            NewVarName = null;
        }
        
        public VariableUpdateData(int type, string varName, string newVarName)
        {
            Type = type;
            VarName = varName;
            NewVarName = newVarName;
        }
    }
    
    public class VariableMap : Observable<VariableUpdateData>
    {
        public Dictionary<string, List<VariableModel>> mVariableMap;
        private Workspace mWorkspace;

        /// <summary>
        /// Class for a vriable map. This contains a dictionary data structure with 
        /// variable types as keys and lists of variables as values. The list of 
        /// variables are the type indicated by the key.
        /// </summary>
        /// <param name="workspace"> The workspace this map belongs to.</param>
        public VariableMap(Workspace workspace)
        {
            // A map from variable type to list of variable names.
            mVariableMap = new Dictionary<string, List<VariableModel>>();

            // The workspace this map belongs to.
            mWorkspace = workspace;
        }

        /// <summary>
        /// Clear the variable map.
        /// </summary>
        public void Clear()
        {
            mVariableMap.Clear();
        }

        /// <summary>
        /// Rename the given variable by updating its name in the variable map.
        /// </summary>
        public void RenameVariable(VariableModel variable, string newName)
        {
            VariableModel newVariable = this.GetVariable(newName);
            int variableIndex = -1;
            int newVariableIndex = -1;
            string type = "";
            if (variable != null) type = variable.Type;
            else if (newVariable != null) type = newVariable.Type;

            List<VariableModel> varList = this.GetVariablesOfType(type);
            if (variable != null)
                variableIndex = varList.IndexOf(variable);
            if (newVariable != null)
                newVariableIndex = varList.IndexOf(newVariable);

            if (variableIndex == -1 && newVariableIndex == -1)
            {
                CreateVariable(newName, "");
            }
            else if (variableIndex == newVariableIndex || variableIndex != -1 && newVariableIndex == -1)
            {
                var variableToRename = mVariableMap[type][variableIndex];
                FireUpdate(new VariableUpdateData(VariableUpdateData.Rename, variableToRename.Name, newName));
                variableToRename.Name = newName;
            }
            else if (variableIndex != -1 && newVariableIndex != -1)
            {
                var variableToRename = mVariableMap[type][newVariableIndex];
                FireUpdate(new VariableUpdateData(VariableUpdateData.Rename, variableToRename.Name, newName));
                
                var variableToDelete = mVariableMap[type][variableIndex];
                FireUpdate(new VariableUpdateData(VariableUpdateData.Delete, variableToDelete.Name));
                
                variableToRename.Name = newName;
                mVariableMap[type].RemoveAt(variableIndex);
            }
        }
        
        /// <summary>
        /// Create a variable with a given name, optional type, and optional id.
        /// </summary>
        public VariableModel CreateVariable(string name, string optType = null, string optId = null)
        {
            var variable = this.GetVariable(name);
            if (null != variable)
            {
                if (!string.IsNullOrEmpty(optType) && variable.Type != optType)
                {
                    throw new Exception("Variable " + name + " is already in use and its type is "
                                        + variable.Type + " which conflicts with the passed in " +
                                        "type, " + optType + ".");
                }
                if (!string.IsNullOrEmpty(optId) && !string.Equals(variable.ID, optId))
                {
                    throw new Exception("Variable " + name + " is already in use and its id is "
                                        + variable.ID + " which conflicts with the passed in " +
                                        "id, " + optId + ".");
                }
                // The variable already exists and has the same id and type.
                return variable;
            }
            if (!string.IsNullOrEmpty(optId) && null != this.GetVariableById(optId))
            {
                throw new Exception("Variable " + optId + ", is already in use.");
            }
            optId = string.IsNullOrEmpty(optId) ? Utils.GenUid() : optId;
            optType = string.IsNullOrEmpty(optType) ? "" : optType;

            variable = new VariableModel(this.mWorkspace, name, optType, optId);
            // If optType is not a key,create a new list.
            if (!mVariableMap.ContainsKey(optType))
            {
                this.mVariableMap.Add(optType, new List<VariableModel>() {variable});
            }
            else
            {
                // Else append the variable to the preexisting list.
                this.mVariableMap[optType].Add(variable);
            }
            
            FireUpdate(new VariableUpdateData(VariableUpdateData.Create, variable.Name));
            return variable;
        }

        /// <summary>
        /// Delete a variable
        /// </summary>
        /// <param name="variable"> Variable to delete.</param>
        public void DeleteVariable(VariableModel variable)
        {
            var variableList = mVariableMap[variable.Type];
            foreach (var tempVar in variableList)
            {
                if (string.Equals(tempVar.ID, variable.ID))
                {
                    variableList.Remove(tempVar);
                    FireUpdate(new VariableUpdateData(VariableUpdateData.Delete, variable.Name));
                    return;
                }
            }
        }

        /// <summary>
        /// Find the variable by the given name and return it.Return null if it is not
        ///     found.
        /// </summary>
        /// <param name="name"> The name to check for.</param>
        /// <returns> The variable with the given name, or null if it was not found.</returns>
        public VariableModel GetVariable(string name)
        {
            foreach (var key in mVariableMap.Keys)
            {
                foreach (var variable in mVariableMap[key])
                {
                    if (Names.Equals(variable.Name, name))
                    {
                        return variable;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the variable by the given id and return it.Return null if it is not
        /// found.
        /// </summary>
        /// <param name="id"> The id to check for.</param>
        /// <returns> The variable with the given id.</returns>
        public VariableModel GetVariableById(string id)
        {
            foreach (var key in mVariableMap.Keys)
            {
                foreach (var variable in mVariableMap[key])
                {
                    if (string.Equals(variable.ID, id))
                    {
                        return variable;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get a list containing all of the variables of a specified type.If type is
        /// null,return list of variables with empty stringtype.
        /// </summary>
        /// <param name="type"> Type of the variables to find.</param>
        /// <returns>a shallow copy of variable list of type</returns>
        public List<VariableModel> GetVariablesOfType(string type)
        {
            List<VariableModel> list = new List<VariableModel>();
            List<VariableModel> variableList;
            if (mVariableMap.TryGetValue(string.IsNullOrEmpty(type) ? "" : type, out variableList))
            {
                list.AddRange(variableList);
            }
            return list;
        }

        /// <summary>
        /// Return all variable types.
        /// </summary>
        public List<string> GetVariableTypes()
        {
            return mVariableMap.Keys.ToList();
        }

        /// <summary>
        /// Return all variables of all types.
        /// </summary>
        public List<VariableModel> GetAllVariables()
        {
            List<VariableModel> allVars = new List<VariableModel>();
            foreach (List<VariableModel> list in mVariableMap.Values)
            {
                allVars.AddRange(list);
            }
            return allVars;
        }
    }
}
