using System;

namespace PTGame.Blockly
{
    public sealed class FieldNumber : FieldTextInput
    {
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