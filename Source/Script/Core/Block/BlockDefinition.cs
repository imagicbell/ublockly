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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public class BlockDefinition
    {
        private JObject mJson;
        
        private string mTypeName;
        public string TypeName { get { return mTypeName; } }

        private bool mHasOutput;
        private bool mHasPreviousStatement;
        private bool mHasNextStatement;
        private string[] mOutputChecks;
        private string[] mPreviousStatementChecks;
        private string[] mNextStatementChecks;

        private bool mInputsInlineDefault;

        private string mMutatorName;
        private List<string> mExtensionNames;

        /// <summary>
        /// Initializes the definition from a string of JSON.
        /// </summary>
        public BlockDefinition(string jsonStr) : this(JObject.Parse(jsonStr))
        {
        }

        /// <summary>
        /// Initializes the definition from a JSON object.
        /// </summary>
        public BlockDefinition(JObject json)
        {
            mJson = json;
            
            mTypeName = json["type"].ToString();
            if (string.IsNullOrEmpty(mTypeName))
                throw new Exception("Block definition in Json array is missing a type attribute.Skipping.");

            try
            {
                mHasOutput = mJson.JsonDataContainsKey("output");
                mHasPreviousStatement = mJson.JsonDataContainsKey("previousStatement");
                mHasNextStatement = mJson.JsonDataContainsKey("nextStatement");
                if (mHasOutput && mHasPreviousStatement)
                    throw new Exception("Block cannot have both \"output\" and \"previousStatement\"");

                mOutputChecks = mHasOutput ? ParseChecks(mJson, "output") : null;
                mPreviousStatementChecks = mHasPreviousStatement ? ParseChecks(mJson, "previousStatement") : null;
                mNextStatementChecks = mHasNextStatement ? ParseChecks(mJson, "nextStatement") : null;

                mInputsInlineDefault = ParseInputsInline(mJson, "inputsInline");

                mMutatorName = mJson["mutator"] != null ? mJson["mutator"].ToString() : null;
                mExtensionNames = ParseExtensions(mJson, "extensions");
            }
            catch (JsonException e)
            {
                throw new Exception(string.Format("Cannot load BlockDefinition \"{0}\" from JSON. \n{1}", mTypeName, e));
            }
        }

        public Connection CreateOutputConnection()
        {
            if (!mHasOutput) return null;
            
            Connection connection = new Connection(Define.EConnection.OutputValue);
            connection.SetCheck(mOutputChecks != null ? mOutputChecks.ToList() : null);
            return connection;
        }

        public Connection CreatePreviousStatementConnection()
        {
            if (!mHasPreviousStatement) return null;

            Connection connection = new Connection(Define.EConnection.PrevStatement);
            connection.SetCheck(mPreviousStatementChecks != null ? mPreviousStatementChecks.ToList() : null);
            return connection;
        }

        public Connection CreateNextStatementConnection()
        {
            if (!mHasNextStatement) return null;

            Connection connection = new Connection(Define.EConnection.NextStatement);
            connection.SetCheck(mNextStatementChecks != null ? mNextStatementChecks.ToList() : null);
            return connection;
        }

        public List<Input> CreateInputList()
        {
            List<Input> inputs = new List<Input>();
            List<Field> fields = new List<Field>();

            int i = 0;
            while (!mJson["message" + i].IsNullOrUndefined())
            {
                string message = mJson["message" + i].ToString();
                JArray args = mJson["args" + i] as JArray;

                List<string> tokens = Utils.TokenizeInterpolation(message);
                Dictionary<string, bool> indexDup = new Dictionary<string, bool>();
                foreach (string token in tokens)
                {
                    int tokenNum;
                    if (int.TryParse(token, out tokenNum))
                    {
                        if (tokenNum <= 0 || tokenNum > args.Count)
                        {
                            throw new Exception("Block " + mTypeName + ": Message index %" + token + " out of range");
                        }
                        if (indexDup.ContainsKey(token))
                        {
                            throw new Exception("Block " + mTypeName + ": Message index %" + token + " duplicated.");
                        }
                        indexDup[token] = true;

                        JObject element = args[tokenNum - 1] as JObject;
                        if (element == null)
                            throw new Exception("Block " + mTypeName + " Error reading arg %" + i);

                        while (element != null)
                        {
                            string elementType = element["type"].ToString();
                            if (Define.FIELD_TYPES.Contains(elementType))
                            {
                                fields.Add(FieldFactory.CreateFromJson(element));
                                break;
                            }
                            else if (Define.INPUT_TYPES.Contains(elementType))
                            {
                                Input input = InputFactory.CreateFromJson(element);
                                foreach (Field field in fields)
                                    input.AppendField(field);
                                fields.Clear();
                                inputs.Add(input);
                                break;
                            }
                            else
                            {
                                element = element["alt"] as JObject;
                            }
                        }
                    }
                    else
                    {
                        string newtoken = token.Trim();
                        if (!string.IsNullOrEmpty(newtoken))
                            fields.Add(new FieldLabel(null, newtoken));
                    }
                }

                // If there were leftover fields we need to add a dummy input to hold them.
                if (fields.Count > 0)
                {
                    JObject dummyInputJson = new JObject();
                    dummyInputJson["type"] = "input_dummy";
                    if (mJson["lastDummyAlign" + i] != null)
                    {
                        dummyInputJson["align"] = mJson["lastDummyAlign" + i];
                    }
                    Input input = InputFactory.CreateFromJson(dummyInputJson);
                    foreach (Field field in fields)
                        input.AppendField(field);
                    fields.Clear();
                    inputs.Add(input);
                }
                
                i++;
            }

            return inputs;
        }

        public Mutator CreateMutator()
        {
            if (!string.IsNullOrEmpty(mMutatorName))
                return MutatorFactory.Create(mMutatorName);
            return null;
        }

        public bool GetInputsInlineDefault()
        {
            return mInputsInlineDefault;
        }

        private string[] ParseChecks(JObject json, string key)
        {
            JToken checkObj = json[key];
            if (checkObj == null)
                return null;
            if (checkObj.Type == JTokenType.String)
                return new string[]{checkObj.ToString()};

            if (checkObj.Type == JTokenType.Array)
            {
                JArray jsonChecks = checkObj as JArray;
                string[] checks = new string[jsonChecks.Count];
                for (int i = 0; i < checks.Length; i++)
                {
                    checks[i] = jsonChecks[i].ToString();
                }
                return checks;
            }
            
            return null;
        }

        private int ParseColor(JObject json, string key)
        {
            JToken colorObj = json[key];
            if (colorObj == null) 
                return 0;
            
            int color = 0;
            if (colorObj.Type == JTokenType.String)
            {
                string colorStr = Utils.ReplaceMessageReferences(colorObj.ToString());
                if (!int.TryParse(colorStr, out color))
                    throw new Exception("Block json definition error with color value");
            }
            else if (colorObj is JValue)
            {
                color = (int) ((JValue) colorObj).Value;
            }
            return color;
        }

        private bool ParseInputsInline(JObject json, string key)
        {
            bool inputsInline = false;
            JToken valueObj = json[key];
            if (valueObj != null && valueObj is JValue)
            {
                inputsInline = (bool) ((JValue) valueObj).Value;
            }
            return inputsInline;
        }

        private List<string> ParseExtensions(JObject json, string key)
        {
            JToken extensionObj = json[key];
            List<string> extensions = new List<string>();

            if (extensionObj == null)
                return extensions;

            if (extensionObj.Type == JTokenType.Array)
            {
                JArray array = extensionObj as JArray;
                for (int i = 0; i < array.Count; i++)
                {
                    extensions.Add(array[i].ToString());
                }
            }
            else if (extensionObj.Type == JTokenType.String)
            {
                extensions.Add(extensionObj.ToString());
            }
            else
            {
                throw new Exception("Type \"" + mTypeName + "\": Extensions attribute in JSON expected an array");
            }
            return extensions;
        }
    }
}
