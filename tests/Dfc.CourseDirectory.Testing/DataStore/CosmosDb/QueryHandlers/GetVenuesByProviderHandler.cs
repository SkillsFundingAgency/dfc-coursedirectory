using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetVenuesByProviderHandler : ICosmosDbQueryHandler<GetVenuesByProvider, IReadOnlyCollection<Venue>>
    {
        public IReadOnlyCollection<Venue> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetVenuesByProvider request)
        {
            return inMemoryDocumentStore.Venues.All.Where(v => v.Ukprn == request.ProviderUkprn && v.Status == VenueStatus.Live).ToList();
        }
    }
}
