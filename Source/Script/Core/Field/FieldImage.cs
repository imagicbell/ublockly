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

namespace UBlockly
{
    public sealed class FieldImage : Field
    {
        [FieldCreator(FieldType = "field_image")]
        private static FieldImage CreateFromJson(JObject json)
        {
            string fieldName = json["name"].IsString() ? json["name"].ToString() : "FIELDNAME_DEFAULT";
            string imageSrc = json.JsonDataContainsKey("src") ? json["src"].ToString() : null;
            Number width = json.JsonDataContainsKey("width") ? new Number(json["width"].ToString()) : Number.NaN;
            Number height = json.JsonDataContainsKey("height") ? new Number(json["height"].ToString()) : Number.NaN;
            string alt = json.JsonDataContainsKey("alt") ? json["alt"].ToString() : null;
            return new FieldImage(fieldName, imageSrc, new Vector2<int>((int) width.Value, (int) height.Value), alt);
        }

        private Vector2<int> mSize;
        public Vector2<int> Size { get { return mSize; } }
        
        public FieldImage(string fieldName, string imageSrc, Vector2<int> imageSize, string optAlt = null) : base(fieldName)
        {
            this.mText = !string.IsNullOrEmpty(optAlt) ? optAlt : "";
            if (string.IsNullOrEmpty(imageSrc))
                imageSrc = "fieldimage_default";
            this.SetValue(imageSrc);

            mSize = imageSize;
            if (mSize.x <= 0) mSize.x = Define.FIELD_IMAGE_WIDTH_DEFAULT;
            if (mSize.y <= 0) mSize.y = Define.FIELD_IMAGE_HEIGHT_DEFAULT;

            IsImage = true;
        }
    }
}