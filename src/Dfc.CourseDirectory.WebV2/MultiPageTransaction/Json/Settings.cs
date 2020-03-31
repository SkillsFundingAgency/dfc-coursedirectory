using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json
{
    public static class Settings
    {
        public static JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto
            };

            settings.Converters.Add(new StandardOrFrameworkJsonConverter());

            return settings;
        }
    }
}
