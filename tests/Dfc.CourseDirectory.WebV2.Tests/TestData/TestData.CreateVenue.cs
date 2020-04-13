using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task<Guid> CreateVenue(
            Guid providerId,
            string venueName = "Test Venue")
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
                VenueName = venueName
            });

            return venueId;
        }
    }
}
