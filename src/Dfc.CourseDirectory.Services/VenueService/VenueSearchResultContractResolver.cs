using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchResultContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
        var names = new Dictionary<string, string>
            {
                { "ID", "id" },
                { "UKPRN", "UKPRN" },
                { "ProviderID", "PROVIDER_ID" },
                { "VenueID", "VENUE_ID" },
                { "Venue_Name", "VENUE_NAME" },
                { "ProvVenueID", "PROV_VENUE_ID" },
                { "Address1", "ADDRESS_1" },
                { "Address2", "ADDRESS_2" },
                { "Town", "TOWN" },
                { "County", "COUNTY" },
                { "PostCode", "POSTCODE" },
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
