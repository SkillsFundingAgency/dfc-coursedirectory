using System;
using Flurl;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json
{
    public class UrlJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Url);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var asString = reader.ReadAsString();
            return new Url(asString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var asString = ((Url)value).ToString();
            writer.WriteValue(asString);
        }
    }
}
