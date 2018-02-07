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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace UBlockly
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MutatorClassAttribute : Attribute
    {
        [Description("mark class for block mutator")]
        public MutatorClassAttribute() {}
        
        /// <summary>
        /// id of mutator, which is defined in json definition
        /// enable multiple mutator id share on mutator class, seperating with ";"
        /// eg. [Mutator(MutatorId = "procedures_defnoreturn_mutator; procedures_defreturn_mutator")]
        /// </summary>
        public string MutatorId { get; set; }
    }
    
    public static class MutatorFactory
    {
        private static Dictionary<string, Type> mMutatorDict = null;
        /// <summary>
        /// mutator factory method
        /// </summary>
        public static Mutator Create(string mutatorId)
        {
            if (mMutatorDict == null)
            {
                mMutatorDict = new Dictionary<string, Type>();
                Assembly assem = Assembly.GetAssembly(typeof(Field));
                foreach (Type type in assem.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(Mutator)))
                    {
                        var attrs = type.GetCustomAttributes(typeof(MutatorClassAttribute), false);
                        if (attrs.Length > 0)
                        {
                            string mutatorIdStr = ((MutatorClassAttribute) attrs[0]).MutatorId;
                            string[] strs = mutatorIdStr.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < strs.Length; i++)
                            {
                                mMutatorDict[strs[i]] = type;
                            }
                        }
                    }
                }
            }

            Type mutatorType;
            if (!mMutatorDict.TryGetValue(mutatorId, out mutatorType))
                throw new Exception(string.Format(
                    "There is no class implementation defined for mutator id: \"{0}\", or you might forget to add a \"MutatorClassAttribute\" to the class.",
                    mutatorId));
            Mutator mutator = Activator.CreateInstance(mutatorType) as Mutator;
            mutator.MutatorId = mutatorId;
            return mutator;
        }
    }
}
