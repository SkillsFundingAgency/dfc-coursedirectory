using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task<Venue> CreateVenue(
            Guid providerId,
            UserInfo createdBy,
            string venueName = "Test Venue",
            string providerVenueRef = null,
            string addressLine1 = "Venue address line 1",
            string addressLine2 = "",
            string town = "Town",
            string county = "",
            string postcode = "AB1 2DE",
            decimal latitude = 1,
            decimal longitude = 1,
            string email = "",
            string telephone = "",
            string website = "",
            DateTime? createdOn = null)
        {
            return WithSqlQueryDispatcher(async dispatcher =>
            {
                var venueId = Guid.NewGuid();

                await dispatcher.ExecuteQuery(new CreateVenue()
                {
                    VenueId = venueId,
                    ProviderId = providerId,
                    ProviderVenueRef = providerVenueRef,
                    Name = venueName,
                    Email = email,
                    Telephone = telephone,
                    Website = website,
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    Town = town,
                    County = county,
                    Postcode = postcode,
                    Latitude = latitude,
                    Longitude = longitude,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn ?? _clock.UtcNow
                });

                var venue = await dispatcher.ExecuteQuery(new GetVenue() { VenueId = venueId });

                return venue;
            });
        }
    }
}
