/****************************************************************************

Utility functions for handling variables and procedure names.

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
using System.Text.RegularExpressions;

namespace UBlockly
{
    /// <summary>
    /// Class for a database of entity names (variables, functions, etc).
    /// todo: add variable prefix
    /// </summary>
    public class Names
    {
        private Dictionary<string, bool> mReservedDict = null;
        private Dictionary<string, bool> mDBReserve = null;
        private Dictionary<string, string> mDB = null;
        
        /// <summary>
        /// </summary>
        /// <param name="reservedWords">A comma-separated string of words that are illegal for use as names in a language (e.g. 'new,if,this,...').</param>
        public Names(string reservedWords)
        {
            mReservedDict = new Dictionary<string, bool>();
            AddReservedWords(reservedWords);
         
            mDB = new Dictionary<string, string>();
            mDBReserve = new Dictionary<string, bool>();
        }

        public void Reset()
        {
            mDB.Clear();
            mDBReserve.Clear();
        }

        public void AddReservedWords(string reservedWords)
        {
            if (string.IsNullOrEmpty(reservedWords))
                return;
            
            string[] strs = reservedWords.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i++)
            {
                mReservedDict[strs[i].Trim()] = true;
            }
        }

        /// <summary>
        /// Check if the variable name has defined
        /// </summary>
        public bool ExistName(string name, string type)
        {
            string normalized = name.ToLower() + "_" + type;
            return mDB.ContainsKey(normalized);
        }

        /// <summary>
        /// Convert a Blockly entity name to a legal exportable entity name.
        /// </summary>
        public string GetName(string name, string type)
        {
            string normalized = name.ToLower() + "_" + type;
            string safeName;
            if (!mDB.TryGetValue(normalized, out safeName))
            {
                safeName = GetDistinctName(name);
                mDB[normalized] = safeName;
            }
            return safeName;
        }

        /// <summary>
        /// Convert a Blockly entity name to a legal exportable entity name.
        /// Ensure that this is a new name not overlapping any previously defined name.
        /// Also check against list of reserved words for the current language and ensure name doesn't collide.
        /// </summary>
        public string GetDistinctName(string name)
        {
            var safeName = GetSafeName(name);
            string i = "";
            while (mDBReserve.ContainsKey(safeName + i) || mReservedDict.ContainsKey(safeName + i))
            {
                i = string.IsNullOrEmpty(i) ? "2" : (int.Parse(i) + 1).ToString();
            }
            safeName += i;
            mDBReserve[safeName] = true;
            return safeName;
        }

        /// <summary>
        /// remove a distinct name form DB
        /// </summary>
        public void RemoveDistinctName(string distinctName)
        {
            mDBReserve.Remove(distinctName);
        }

        /// <summary>
        /// Given a proposed entity name, generate a name that conforms to the [_A-Za-z][_A-Za-z0-9]* format 
        /// that most languages consider legal for variables.
        /// </summary>
        public static string GetSafeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "unnamed";

            // remove the check of invalid characters in C#.
            // name = Uri.EscapeUriString(name.Replace(" ", "_"));
            // Regex regex = new Regex(@"[^\w]");
            // regex.Replace(name, "_");

            // if ("0123456789".IndexOf(name[0]) != -1)
            //     name = "my_" + name;
            
            return name;
        }
        
        public static bool IsSafe(string name)
        {
            return !string.IsNullOrEmpty(name) && name.Trim().Length == name.Length;
        }

        /// <summary>
        /// Do the given two entity names refer to the same entity?
        /// Blockly names are case-insensitive.
        /// </summary>
        /// <param name="name1"> Frist name.</param>
        /// <param name="name2"> Second name</param>
        /// <returns> True if names are the same.</returns>
        public static bool Equals(string name1, string name2)
        {
            if (name1 != null && name2 != null)
            {
                return string.Equals(name1.ToLower(), name2.ToLower());
            }
            return false;
        }
    }
}
