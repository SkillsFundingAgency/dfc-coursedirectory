using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class CreateVenueHandler : ICosmosDbQueryHandler<CreateVenue, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateVenue request)
        {
            var venue = new Venue()
            {
                Id = request.VenueId,
                Status = 1,
                UKPRN = request.ProviderUkprn,
                VenueName = request.VenueName
            };
            inMemoryDocumentStore.Venues.Save(venue);

            return new Success();
    }
}
}
