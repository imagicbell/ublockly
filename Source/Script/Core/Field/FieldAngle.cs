using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UBlockly
{
    public class FieldAngle : FieldTextInput
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

        /// <summary>
        /// CLOCKWISE and OFFSET work together to set the behaviour of the angle picker.  
        /// While many combinations are possible, two modes are typical: 
        /// Math mode. 0 deg is right, 90 is up.  This is the style used by protractors.
        ///    CLOCKWISE = false;
        ///    OFFSET = 0; 
        /// Compass mode. 0 deg is up, 90 is right.  This is the style used by maps.
        ///    CLOCKWISE = true; 
        ///    OFFSET = 90;
        /// </summary>
        public static bool CLOCKWISE = false;
        public static int OFFSET = 0;

        /// <summary>
        /// Maximum allowed angle before wrapping.
        /// Usually either 360 (for 0 to 359.9) or 180 (for -179.9 to 180).
        /// </summary>
        public static int WRAP = 360;

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

            if (mAngleNumber.Value > WRAP)
                mAngleNumber.Value -= 360;

            return mAngleNumber.ToString();
        }
    }
}
