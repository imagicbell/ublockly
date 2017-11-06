using Newtonsoft.Json.Linq;

namespace PTGame.Blockly
{
    public static class FieldFactory
    {
        /// <summary>
        /// create field from json object
        /// </summary>
        public static Field CreateFromJson(JObject json)
        {
            string fieldType = json["type"].ToString();
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            Field field = null;
            switch (fieldType)
            {
                case "field_label":
                {
                    var text = json["text"].IsString() ? Utils.ReplaceMessageReferences(json["text"].ToString()) : "";
                    field = new FieldLabel(fieldName, text);
                    break;
                }
                case "field_input":
                {
                    var text = json["text"].IsString() ? Utils.ReplaceMessageReferences(json["text"].ToString()) : "";
                    field = new FieldTextInput(fieldName, text);
                    break;
                }
                case "field_angle":
                {
                    field = new FieldAngle(fieldName, json["angle"].ToString());
                    break;
                }
                case "field_checkbox":
                {
                    field = new FieldCheckbox(fieldName, json["checked"] != null ? "TRUE" : "FALSE");
                    break;
                }
                case "field_colour":
                {
                    field = new FieldColour(fieldName, json["colour"].ToString());
                    break;
                }
                case "field_variable":
                {
                    var varName = json["variable"].IsString() ? Utils.ReplaceMessageReferences(json["variable"].ToString()) : "";
                    field = new FieldVariable(fieldName, varName);
                    break;
                }
                case "field_dropdown":
                {
                    field = new FieldDropdown(fieldName, json["options"] as JArray);
                    break;
                }
                case "field_number":
                {
                    field = new FieldNumber(fieldName,
                                            json["value"].ToString(), 
                                            json["min"] == null ? null : json["min"].ToString(),
                                            json["max"] == null ? null : json["max"].ToString());
                    break;
                }
                case "field_image":
                {
                    //todo: 
                    break;
                }
                case "field_date":
                {
                    //todo:
                    break;
                }
            }

            return field;
        }
    }
}