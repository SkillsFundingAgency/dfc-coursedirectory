using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class AddressSelectionResultContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            var names = new Dictionary<string, string>
            {
                { "Id", "id" },
                { "Line1", "line1"},
                { "Line2", "line2"},
                { "Line3", "line3"},
                { "Line4", "line4"},
                { "City", "city"},
                { "PostCode", "postcode"},
                { "Province", "province"}
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