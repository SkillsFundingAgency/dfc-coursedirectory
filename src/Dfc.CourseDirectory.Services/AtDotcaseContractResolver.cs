using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class AtDotcaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            var names = new Dictionary<string, string>
            {
                { "ODataContext", "@odata.context" },
                { "ODataCount", "@odata.count" },
                { "SearchFacets", "@search.facets" },
                { "NotionalNVQLevelv2ODataType", "NotionalNVQLevelv2@odata.type" },
                { "AwardOrgCodeODataType", "AwardOrgCode@odata.type" }
            };

            if (names.ContainsKey(propertyName))
            {
                names.TryGetValue(propertyName, out string name);
                return name;
            }

            return propertyName;

            //var name = propertyName
            //    .Replace("OData", "odata")
            //    .ToDotcase();

            //return name.Contains(".") ? $"@{name}" : name;
        }
    }

    internal static class StringExtensions
    {
        internal static string ToDotcase(this string str)
        {
            return string.Concat(
                str.Select((x, i) => 
                    i > 0 && char.IsUpper(x) 
                        ? "." + x.ToString() 
                        : x.ToString()))
                .ToLower();
        }
    }
}
