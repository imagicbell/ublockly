/****************************************************************************

Utility functions for interpreting Blockly code to implementations

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
using System.Reflection;

namespace UBlockly
{
    public abstract class Interpreter
    {
        public abstract CodeName Name { get; }
        
        /// <summary>
        /// instances for interpreting code
        /// </summary>
        protected Dictionary<string, Cmdtor> mCmdMap;

        protected Interpreter()
        {
            InitCodeDB();
        }
        
        /// <summary>
        /// collect all code generation/interpretion methods
        /// </summary>
        protected void InitCodeDB()
        {
            mCmdMap = new Dictionary<string, Cmdtor>();
            Assembly assem = Assembly.GetAssembly(this.GetType());
            foreach (Type type in assem.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Cmdtor)))
                {
                    var attrs = type.GetCustomAttributes(typeof(CodeInterpreterAttribute), false);
                    if (attrs.Length > 0)
                    {
                        mCmdMap[((CodeInterpreterAttribute) attrs[0]).BlockType] = Activator.CreateInstance(type) as Cmdtor;
                    }
                }
            }
        }
    }
}
