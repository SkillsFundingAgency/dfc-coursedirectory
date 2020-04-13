using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllVenuesForProviderHandler :
        ICosmosDbQueryHandler<GetAllVenuesForProvider, IReadOnlyCollection<Venue>>
    {
        public IReadOnlyCollection<Venue> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            GetAllVenuesForProvider request)
        {
            return inMemoryDocumentStore.Venues.All.Where(v => v.UKPRN == request.ProviderUkprn).ToList();
        }
    }
}
