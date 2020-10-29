
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;


namespace Dfc.CourseDirectory.FindACourseApi.Services
{
    public class PostcodeSearchResultContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            var names = new Dictionary<string, string>
            {
                { "ODataContext", "@odata.context" },
                { "ODataCount", "@odata.count" },
                { "SearchFacets", "@search.facets" },
                { "SearchScore", "@search.score" }
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