using System;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public sealed class FieldNumber : FieldTextInput
    {
        [FieldCreator(FieldType = "field_number")]
        private static FieldNumber CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            return new FieldNumber(fieldName,
                                    json["value"].ToString(),
                                    json["min"] == null ? null : json["min"].ToString(),
                                    json["max"] == null ? null : json["max"].ToString());
        }


        private Number mNumber;
        
        private Number mMin;
        public Number Min { get { return mMin; } }

        private Number mMax;
        public Number Max { get { return mMax; } }

        /// <summary>
        /// Class for an editable number field.
        /// </summary>
        public FieldNumber(string fieldName) : this(fieldName, "0") {}

        /// <summary>
        /// Class for an editable number field.
        /// </summary>
        public FieldNumber(string fieldName, string optValue, string optMin = null, string optMax = null) : base(fieldName)
        {
            mNumber = new Number(!string.IsNullOrEmpty(optValue) ? optValue : "0");
            if (mNumber.IsNaN) mNumber = new Number(0);
            this.SetValue(mNumber.ToString());

            mMin = !string.IsNullOrEmpty(optMin) ? new Number(optMin) : Number.MinValue;
            mMax = !string.IsNullOrEmpty(optMax) ? new Number(optMax) : Number.MaxValue;
            SetValue(CallValidator(GetValue()));
        }

        /// <summary>
        /// Class for an editable number field.
        /// Please input a Number value instantiated of Number Type
        /// </summary>
        public FieldNumber(string fieldName, Number optValue, Number optMin, Number optMax) : base(fieldName)
        {
            mNumber = optValue.IsNaN ? new Number(0) : optValue;
            this.SetValue(mNumber.ToString());

            mMin = optMin.IsNaN ? Number.MinValue : optMin;
            mMax = optMax.IsNaN ? Number.MaxValue : optMax;
            SetValue(CallValidator(GetValue()));
        }

        protected override string ClassValidator(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            text = text.Replace(",", "");
            mNumber = new Number(text);
            if (mNumber.IsNaN)
            {
                // Invalid number.
                return null;
            }

            mNumber.Clamp(mMin, mMax);
            return mNumber.ToString();
        }
    }
}
