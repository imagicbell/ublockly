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
            FieldAngle field = new FieldAngle(fieldName, json["angle"].ToString());
            if (json["gap"] != null)
                field.mGap = new Number(json["gap"].ToString());
            return field;
        }

        
        private Number mAngleNumber;
        private Number mOriAngleNumber;
        
        /// <summary>
        /// gap between angles. eg. 0, 30, 60...
        /// </summary>
        private Number mGap;
        public Number Gap { get { return mGap; } }

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

            mOriAngleNumber = mAngleNumber;
            mGap = new Number(0);
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

            if (mGap.Value > 0)
            {
                int interval = Mathf.FloorToInt((mAngleNumber.Value - mOriAngleNumber.Value) / mGap.Value);
                mAngleNumber.Value = mOriAngleNumber.Value + interval * mGap.Value;
            }
            
            return mAngleNumber.ToString();
        }
    }
}
