using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetVenueByIdHandler : ICosmosDbQueryHandler<GetVenueById, Venue>
    {
        public Venue Execute(InMemoryDocumentStore inMemoryDocumentStore, GetVenueById request) =>
            inMemoryDocumentStore.Venues[request.VenueId.ToString()];
    }
}
