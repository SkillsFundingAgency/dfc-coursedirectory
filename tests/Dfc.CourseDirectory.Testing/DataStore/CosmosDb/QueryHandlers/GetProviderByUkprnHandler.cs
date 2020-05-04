using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByUkprnHandler : ICosmosDbQueryHandler<GetProviderByUkprn, Provider>
    {
        public Provider Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderByUkprn request) =>
            inMemoryDocumentStore.Providers.All
                .SingleOrDefault(p => p.UnitedKingdomProviderReferenceNumber == request.Ukprn.ToString());
    }
}
