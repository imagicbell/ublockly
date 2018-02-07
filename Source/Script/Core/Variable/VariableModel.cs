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

namespace UBlockly
{
    public class VariableModel
    {
        /// <summary>
        /// The workspace the variable is in.
        /// </summary>
        public Workspace Workspace;

        /// <summary>
        /// The name of the variable, typically defined by the user.It must be 
        /// unique across all names used for procedures and variables.It main be 
        /// changed by the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the variable,such as 'int' or 'sound_effect'. This may be
        /// used to build a list of variables of a specific type. By default this is
        /// the empty string '', which is a specific type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A unique id for the variable. This should be defined at creation and
        /// not change,even if the nam changes. In most cases this should be a
        /// UUID. 
        /// </summary>
        public string ID { get; set; }
        
        /// <summary>
        /// Class for a variable model.
        /// Holds informatin for the variable including name,id,and type.
        /// </summary>
        /// <param name="workspace"> The variable's workspace.</param>
        /// <param name="name"> name The name of the variable. This must be unique across
        /// variables and procedures.</param>
        /// <param name="optType"> The type of the variable like 'int' or 'string'.
        /// Does not need to be unique. Field_variable can filter variables based on 
        /// their type.This will default to '' which is a specific type.</param>
        /// <param name="optId"> The unique id of the variable. This will default to a UUID.</param>
        public VariableModel(Workspace workspace, string name, string optType = null, string optId = null)
        {
            this.Workspace = workspace;
            this.Name = name;
            this.Type = string.IsNullOrEmpty(optType) ? "" : optType;
            this.ID = string.IsNullOrEmpty(optId) ? Utils.GenUid() : optId;
        }

        /// <summary>
        /// A custom compare function for the VariableModel objects.
        /// </summary>
        public static int CompareByName(VariableModel var1, VariableModel var2)
        {
            return String.CompareOrdinal(var1.Name.ToLower(), var2.Name.ToLower());
        }
    }
}
