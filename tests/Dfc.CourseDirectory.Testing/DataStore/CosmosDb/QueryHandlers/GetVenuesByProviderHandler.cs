using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetVenuesByProviderHandler : ICosmosDbQueryHandler<GetVenuesByProvider, IReadOnlyCollection<Venue>>
    {
        public IReadOnlyCollection<Venue> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetVenuesByProvider request)
        {
            return inMemoryDocumentStore.Venues.All.Where(v => v.Ukprn == request.ProviderUkprn).ToList();
        }
    }
}
