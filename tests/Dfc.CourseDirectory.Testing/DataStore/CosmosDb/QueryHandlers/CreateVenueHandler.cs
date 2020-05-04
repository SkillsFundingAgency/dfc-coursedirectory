using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class CreateVenueHandler : ICosmosDbQueryHandler<CreateVenue, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateVenue request)
        {
            var venue = new Venue()
            {
                Id = request.VenueId,
                Status = 1,
                Ukprn = request.ProviderUkprn,
                VenueName = request.VenueName
            };
            inMemoryDocumentStore.Venues.Save(venue);

            return new Success();
    }
}
}
