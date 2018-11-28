using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class VenueSearchResultContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
        var names = new Dictionary<string, string>
            {
                { "ID", "id" },
                { "UKPRN", "ukprn" },
                { "ProviderID", "provideR_ID" },
                { "VenueID", "venuE_ID" },
                { "VenueName", "venuE_NAME" },
                { "ProvVenueID", "proV_VENUE_ID" },
                { "Address1", "addresS_1" },
                { "Address2", "addresS_2" },
                { "Town", "town" },
                { "County", "county" },
                { "PostCode", "postcode" },
                { "Value", "value"}
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
