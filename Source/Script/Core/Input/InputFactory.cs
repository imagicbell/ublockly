using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PTGame.Blockly
{
    public static class InputFactory
    {
        /// <summary>
        /// create input from json object
        /// </summary>
        public static Input CreateFromJson(JObject json)
        {
            string inputType = json["type"].ToString();
            int inputTypeInt = Blockly.INPUT_VALUE;
            string inputName = json["name"] != null ? json["name"].ToString() : "";
            Connection connection = null;
            switch (inputType)
            {
                case "input_value":
                    inputTypeInt = Blockly.INPUT_VALUE;
                    connection = new Connection(inputTypeInt);
                    break;
                case "input_statement":
                    inputTypeInt = Blockly.NEXT_STATEMENT;
                    connection = new Connection(inputTypeInt);
                    break;
                case "input_dummy":
                    inputTypeInt = Blockly.DUMMY_INPUT;
                    break;
            }
            
            Input input = new Input(inputTypeInt, inputName, connection);
            if (json["align"] != null)
            {
                string alignText = json["align"].ToString();
                int align = alignText.Equals("LEFT") 
                    ? Blockly.ALIGN_LEFT 
                    : (alignText.Equals("RIGHT") ? Blockly.ALIGH_RIGHT : Blockly.ALIGN_CENTER);
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
        public static Input Create(int type, string name, int align, List<string> check)
        {
            Connection connection = (type == Blockly.INPUT_VALUE || type == Blockly.NEXT_STATEMENT) ? new Connection(type) : null;
            Input input = new Input(type, name, connection);
            input.SetAlign(align);
            input.SetCheck(check);
            return input;
        }
    }
}