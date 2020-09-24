/****************************************************************************

Copyright 2016 liangxiegame@163.com
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
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

namespace UBlockly
{
    public static class Utils
    {
        public static bool isSymbol(string word)
        {
            //char dd = Convert.ToChar(word);
            Regex rx = new Regex("^[\u4e00-\u9fa5]$");
            if (rx.IsMatch(word))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string StringSplit(string src)
        {
            string res = "";
            for (int i = 0; i < src.Length; ++i)
            {
                if (!isSymbol(src[i].ToString()))
                {
                    res += src[i];
                }
            }
            return res;
        }


        /// <summary>
        /// Replaces string table references in a message , if the message is a string.
        /// For example,"%{bky_my_msg}" and "%{BKY_MY_MSG}" will both be replaced with
        /// the value in I18n.Msg['MY_MSG'].
        /// </summary>
        public static string ReplaceMessageReferences(JObject message)
        {
            if (!message.IsString())
                return message.ToString();

            return ReplaceMessageReferences(message.ToString());
        }

        /// <summary>
        /// Replaces string table references in a message , if the message is a string.
        /// For example,"%{bky_my_msg}" and "%{BKY_MY_MSG}" will both be replaced with
        /// the value in I18n.Msg['MY_MSG'].
        /// </summary>
        public static string ReplaceMessageReferences(string message)
        {
            var interpolatedResult = TokenizeInterpolation(message, false);
            // When parseInterpolationTOkens == false,interpolatedResult should be at
            // most length 1
            return interpolatedResult.Count > 0 ? interpolatedResult[0] : ""; 
        }

        /// <summary>
        /// Internal implemention of the message reference and interpolation token
        /// Parsing used by tokenizeInterpolation() and replaceMessageReferences().
        /// </summary>
        public static List<string> TokenizeInterpolation(JToken message, bool parseInterpolationTokens = true)
        {
            return Utils.TokenizeInterpolation(message == null ? "" : message.ToString(), parseInterpolationTokens);
        }
        
        /// <summary>
        /// Internal implemention of the message reference and interpolation token
        /// Parsing used by tokenizeInterpolation() and replaceMessageReferences().
        /// </summary>
        public static List<string> TokenizeInterpolation(string message, bool parseInterpolationTokens = true)
        {
            var tokens = new List<string>();
            var chars = message.ToCharArray().ToList();
            // Parse the message with a finite state machine.
            // 0 - Base case.
            // 1 - % found
            // 2 - Digit found.
            // 3 - Message ref found
            var state = 0;
            var buffer = new List<string>();
            string number = "";
            for (var i = 0; i < chars.Count + 1; i++)
            {
                var c = i == chars.Count ? ' ' : chars[i];
                if (state == 0 && i != chars.Count)
                {
                    if (c == '%')
                    {
                        var text = string.Join("", buffer.ToArray());
                        if (!string.IsNullOrEmpty(text))
                        {
                            tokens.Add(text);
                        }
                        buffer.Clear();
                        state = 1; // Start escape.
                    }
                    else
                    {
                        buffer.Add(c.ToString()); // Regular char
                    }
                }
                else if (state == 1)
                {
                    if (c == '%')
                    {
                        buffer.Add(c.ToString()); // Escaped %: %%
                        state = 0;
                    }
                    else if (parseInterpolationTokens && '0' <= c && c <= '9')
                    {
                        state = 2;
                        number = c.ToString();
                        buffer.Add("");
                        var text = string.Join("", buffer.ToArray());
                        if (!string.IsNullOrEmpty(text))
                        {
                            tokens.Add(text);
                        }
                        buffer.Clear();
                    }
                    else if (c == '{')
                    {
                        state = 3;
                    }
                    else 
                    {
                        buffer.Add("%"); // Not recognized. Return as literal.
                        if (i != chars.Count)
                            buffer.Add(c.ToString());
                        state = 0; // add parse as string literl
                    }
                }
                else if (state == 2)
                {
                    if ('0' <= c && c <= '9')
                    {
                        number += c; // Multi-digit number.
                    }
                    else
                    {
                        tokens.Add(int.Parse(number).ToString());
                        i--; // Parse this char agian.
                        state = 0;
                    }
                }
                else if (state == 3)
                {
                    if (i == chars.Count)
                    {
                        // Premature end before closing '}'
                        buffer.Insert(0, "%{"); // Re-insert leading delimiter
                        i--; // Parse this char again.
                        state = 0; // add parse as string literal.
                    }
                    else if (c != '}')
                    {
                        buffer.Add(c.ToString());
                    }
                    else
                    {
                        var rawKey = string.Join("", buffer.ToArray());
                        if (new Regex("[a-zA-Z][a-zAZ0-9_]").Match(rawKey).Success) // Strict matching
                        {
                            // Found a valied string key. Attempt case insensitive match.
                            var keyUpper = rawKey.ToUpper();

                            // BKY_ is the prefix used to namespace the string s used in Blockly
                            // core files adn the predefined blocks in ../blocks/. These strings
                            // are defined in ../msgs/ files.
                            var bklyKey = keyUpper.StartsWith("BKY_") ? keyUpper.Substring(4) : null;
                            if (!string.IsNullOrEmpty(bklyKey) && I18n.Contains(bklyKey))
                            {
                                var rawValue = I18n.Get(bklyKey);
                                tokens.AddRange(TokenizeInterpolation(rawValue));
                            }
                            else
                            {
                                tokens.Add("%{" + rawKey + "}");
                            }

                            buffer.Clear(); // Clear the array
                            state = 0;
                        }
                        else
                        {
                            tokens.Add("%{" + rawKey + "}");
                            buffer.Clear();
                            state = 0; // and parse as string literal.      
                        }
                    }
                }
            }

            var text1 = string.Join("", buffer.ToArray());
            if (!string.IsNullOrEmpty(text1))
            {
                tokens.Add(text1);
            }
            
            // Merge adjacent text tokens into a single string.
            var mergedTokens = new List<string>();
            buffer.Clear();
            for (int i = 0; i < tokens.Count; i++)
            {
                int tokenNum = 0;

                if (int.TryParse(tokens[i], out tokenNum))
                {
                    text1 = string.Join("", buffer.ToArray());
                    if (!string.IsNullOrEmpty(text1))
                    {
                        mergedTokens.Add(text1);
                    }
                    buffer.Clear();
                    mergedTokens.Add(tokens[i]);
                } 
                else
                {
                    buffer.Add(tokens[i]);
                }
            }

            text1 = string.Join("", buffer.ToArray());
            
            if (!string.IsNullOrEmpty(text1))
            {
                mergedTokens.Add(text1);
            }
            buffer.Clear();
            
            // remove last
            return mergedTokens;
        }
        
        
        #if UNITY_EDITOR
        private static bool mGenUidValueDirty = false;
        private static string mEditorDefaultGenUidValue = string.Empty;
        public static string EditorDefaultGenUidValue
        {
            set
            {
                mEditorDefaultGenUidValue = value;
                mGenUidValueDirty = true;
            }
        }

        public static void ResetGenUidValueDirty2False()
        {
            mGenUidValueDirty = false;
        }
        #endif
        /// <summary>
        /// Generate a unique ID. This should be globally unique.
        /// 87 characters ^ 20 length > 128bits (better than a UUID).
        /// </summary>
        /// <returns></returns>
        public static string GenUid()
        {
            #if UNITY_EDITOR
            if (mGenUidValueDirty) return mEditorDefaultGenUidValue;
            #endif
            var length = 20;
            var soupLength = SOUP.Length;
            var id = new List<char>();
            for (int i = 0; i < length; i++)
            {
                id.Add(SOUP[(int) (Random.Range(0.0f, 1.0f) * soupLength)]);
            }
            
            return new string(id.ToArray());
        }
        
        /// <summary>
        /// Legal characters for the unique ID.  Should be all on a US keyboard.
        /// No characters that conflict with XML or JSON.  Requests to remove additional 'problematic' characters from this soup will be denied.  
        /// That's your failure to properly escape in your own environment. 
        /// </summary>
        public static char[] SOUP = "!#$%()*+,-./:;=?@[]^_`{|}~ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        /// <summary>
        /// Given an array of strings, return the length of the shortest one.
        /// </summary>
        public static int ShortestStringLength(string[] strArray)
        {
            if (strArray == null || strArray.Length == 0)
                return 0;
            int minLength = int.MaxValue;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].Length < minLength)
                    minLength = strArray[i].Length;
            }
            return minLength;
        }

        /// <summary>
        /// Given an array of strings, return the length of the common prefix.
        /// Words may not be split.  Any space after a word is included in the length.
        /// </summary>
        public static int CommonWordPrefix(string[] strArray)
        {
            if (strArray == null || strArray.Length == 0)
                return 0;
            if (strArray.Length == 1)
                return strArray[0].Length;

            int maxLength = ShortestStringLength(strArray);
            int len = 0;
            int wordPrefix = 0;
            for (len = 0; len < maxLength; len++)
            {
                char letter = strArray[0][len];
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (letter != strArray[i][len])
                        return wordPrefix;
                }

                if (letter == ' ')
                    wordPrefix = len + 1;
            }
            
            for (int i = 1; i < strArray.Length; i++)
            {
                if (strArray[i].Length > len)
                {
                    char letter = strArray[i][len];
                    if (letter != ' ')
                        return wordPrefix;    
                }
            }
            return maxLength;
        }

        /// <summary>
        /// Given an array of strings, return the length of the common suffix. 
        /// Words may not be split.  Any space after a word is included in the length.
        /// </summary>
        public static int CommonWordSuffix(string[] strArray)
        {
            if (strArray == null || strArray.Length == 0)
                return 0;
            if (strArray.Length == 1)
                return strArray[0].Length;
            
            int maxLength = ShortestStringLength(strArray);
            int len = 0;
            int wordSuffix = 0;
            for (len = 0; len < maxLength; len++)
            {
                char letter = strArray[0][strArray[0].Length - len - 1];
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (letter != strArray[i][strArray[i].Length - len - 1])
                        return wordSuffix;
                }
                if (letter == ' ')
                    wordSuffix = len + 1;
            }

            for (int i = 1; i < strArray.Length; i++)
            {
                if (strArray[i].Length > len)
                {
                    char letter = strArray[i][strArray[i].Length - len - 1];
                    if (letter != ' ')
                        return wordSuffix;
                }
            }
            return maxLength;
        }


    }
}
