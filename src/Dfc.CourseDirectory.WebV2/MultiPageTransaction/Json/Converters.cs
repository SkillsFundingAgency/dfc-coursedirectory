using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json
{
    public static class Converters
    {
        public static JsonConverter[] All { get; } = new[]
        {
            new StandardOrFrameworkJsonConverter()
        };
    }
}
