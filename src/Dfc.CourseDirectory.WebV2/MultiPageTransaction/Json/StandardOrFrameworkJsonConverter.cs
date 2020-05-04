using System;
using Dfc.CourseDirectory.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json
{
    public class StandardOrFrameworkJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(StandardOrFramework);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var wrapper = JObject.Load(reader).ToObject<Wrapper>();

            if (wrapper.IsStandard)
            {
                var standard = wrapper.Value.ToObject<Standard>();
                return new StandardOrFramework(standard);
            }
            else
            {
                var framework = wrapper.Value.ToObject<Framework>();
                return new StandardOrFramework(framework);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var standardOrFramework = (StandardOrFramework)value;

            var wrapper = new Wrapper()
            {
                IsStandard = standardOrFramework.IsStandard,
                Value = JObject.FromObject(standardOrFramework.Value)
            };

            JObject.FromObject(wrapper).WriteTo(writer);
        }

        private class Wrapper
        {
            public bool IsStandard { get; set; }
            public JObject Value { get; set; }
        }
    }
}
