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
using System.Linq;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public static class InputFactory
    {
        /// <summary>
        /// create input from json object
        /// </summary>
        public static Input CreateFromJson(JObject json)
        {
            string inputType = json["type"].ToString();
            Define.EConnection inputTypeInt = Define.EConnection.InputValue;
            string inputName = json["name"] != null ? json["name"].ToString() : "";
            Connection connection = null;
            switch (inputType)
            {
                case "input_value":
                    inputTypeInt = Define.EConnection.InputValue;
                    connection = new Connection(inputTypeInt);
                    break;
                case "input_statement":
                    inputTypeInt = Define.EConnection.NextStatement;
                    connection = new Connection(inputTypeInt);
                    break;
                case "input_dummy":
                    inputTypeInt = Define.EConnection.DummyInput;
                    break;
            }
            
            Input input = new Input(inputTypeInt, inputName, connection);
            if (json["align"] != null)
            {
                string alignText = json["align"].ToString();
                Define.EAlign align = alignText.Equals("LEFT")
                    ? Define.EAlign.Left
                    : (alignText.Equals("RIGHT") ? Define.EAlign.Right : Define.EAlign.Center);
                input.SetAlign(align);
            }
            if (json["check"] != null)
            {
                JArray checkArray = json["check"] as JArray;
                if (checkArray != null)
                {
                    List<string> checkList = checkArray.Select(token => token.ToString()).ToList();
                    input.SetCheck(checkList);
                }
                else
                {
                    input.SetCheck(json["check"].ToString());
                }
            }
            return input;
        }

        /// <summary>
        /// create input from input params
        /// </summary>
        /// <param name="type">input type</param>
        /// <param name="name">input name</param>
        /// <param name="align">input alignment</param>
        /// <param name="check">input type checks</param>
        public static Input Create(Define.EConnection type, string name, Define.EAlign align, List<string> check)
        {
            Connection connection = null;
            if (type == Define.EConnection.InputValue || type == Define.EConnection.NextStatement)
                connection = new Connection(type);
            
            Input input = new Input(type, name, connection);
            input.SetAlign(align);
            input.SetCheck(check);
            return input;
        }
    }
}
