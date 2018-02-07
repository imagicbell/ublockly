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
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public sealed class FieldDate : Field
    {
        [FieldCreator(FieldType = "field_date")]
        private static FieldDate CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            string dateStr = json.JsonDataContainsKey("date") ? json["date"].ToString() : null;
            return new FieldDate(fieldName, dateStr);
        }

        private const string DATE_FORMAT = "yyyy-MM-dd";
        
        private DateTime mDate;
        public DateTime Date { get { return mDate; } }
        
        public FieldDate(string fieldName, string dateStr) : base(fieldName)
        {
            if (!DateTime.TryParseExact(dateStr, DATE_FORMAT, null, DateTimeStyles.None, out mDate))
                throw new Exception(
                    String.Format("FieldDate: can\'t parse date string {0} to DateTime. Correct format is {1}.", dateStr, DATE_FORMAT));
        }
    }
}