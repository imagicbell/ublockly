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