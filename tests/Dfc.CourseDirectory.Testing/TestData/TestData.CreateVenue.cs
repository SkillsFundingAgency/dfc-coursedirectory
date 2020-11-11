using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateVenue(
            Guid providerId,
            string venueName = "Test Venue",
            string addressLine1 = "Venue address line 1",
            string addressLine2 = "",
            string town = "",
            string county = "",
            string postcode = "AB1 2DE",
            decimal latitude = 1,
            decimal longitude = 1,
            string email = "",
            string telephone = "",
            string website = "")
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = providerId
            });

            if (provider == null)
            {
                throw new ArgumentException("Provider does not exist.", nameof(providerId));
            }

            var venueId = Guid.NewGuid();

            await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateVenue()
            {
                VenueId = venueId,
                ProviderUkprn = provider.Ukprn,
                VenueName = venueName,
                Email = email,
                Telephone = telephone,
                Website = website,
                AddressLine1 = addressLine1,
                AddressLine2 = addressLine2,
                Town = town,
                County = county,
                Postcode = postcode,
                Latitude = latitude,
                Longitude = longitude
            });

            var venue = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenueById() { VenueId = venueId });
            await _sqlDataSync.SyncVenue(venue);

            return venueId;
        }
    }
}
