
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;


namespace Dfc.ProviderPortal.FindACourse.Services
{
    public class ProviderSearchResultContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            var names = new Dictionary<string, string>
            {
                { "ODataContext", "@odata.context" },
                { "ODataCount", "@odata.count" },
                { "SearchFacets", "@search.facets" },
                { "TownODataType", "Town@odata.type" },
                { "RegionODataType", "Region@odata.type" },
                { "SearchScore", "@search.score" }
                //{ "ProviderId", "UKPRN" }
            };

            if (names.ContainsKey(propertyName))
            {
                names.TryGetValue(propertyName, out string name);
                return name;
            }

            return propertyName;
        }
    }
}