using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    public sealed class FieldAngle : FieldTextInput
    {
        [FieldCreator(FieldType = "field_angle")]
        private static FieldAngle CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            return new FieldAngle(fieldName, json["angle"].ToString());
        }

        
        private Number mAngleNumber;
        
        /// <summary>
        /// Class for an editable angle field.
        /// </summary>
        public FieldAngle(string fieldName) : this(fieldName, "0") {}

        /// <summary>
        /// Class for an editable angle field.
        /// </summary>
        public FieldAngle(string fieldName, string optValue) : base(fieldName)
        {
            mAngleNumber = new Number(!string.IsNullOrEmpty(optValue) ? optValue : "0");
            if (mAngleNumber.IsNaN) mAngleNumber = new Number(0);
            this.SetValue(mAngleNumber.ToString());
        }

        /// <summary>
        /// Class for an editable angle field.
        /// </summary>
        public FieldAngle(string fieldName, Number optValue) : base(fieldName)
        {
            mAngleNumber = optValue.IsNaN ? new Number(0) : optValue;
            this.SetValue(mAngleNumber.ToString());
        }

        protected override string ClassValidator(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            
            mAngleNumber = new Number(text);
            if (mAngleNumber.IsNaN)
                return null;

            mAngleNumber.Value = mAngleNumber.Value % 360;
            if (mAngleNumber.Value < 0)
                mAngleNumber.Value += 360;

            if (mAngleNumber.Value > Define.FIELD_ANGLE_WRAP)
                mAngleNumber.Value -= 360;

            return mAngleNumber.ToString();
        }
    }
}
