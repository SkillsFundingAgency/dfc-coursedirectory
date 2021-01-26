using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    [JsonConverter(typeof(StringEnumConverter))] // https://stackoverflow.com/questions/45513772/serialize-enum-as-string-in-json-returned-from-azure-function/47564570#47564570
    public enum UnfixableApprenticeshipVenueReasons
    {
        ProviderHasNoLiveVenues
    }
}
