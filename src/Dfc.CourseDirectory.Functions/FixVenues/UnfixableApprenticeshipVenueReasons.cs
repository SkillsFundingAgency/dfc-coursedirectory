using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    [JsonConverter(typeof(StringEnumConverter))] // Use strings in azure function json response. https://stackoverflow.com/a/47564570
    public enum UnfixableApprenticeshipVenueReasons
    {
        ProviderHasNoLiveVenues
    }
}
