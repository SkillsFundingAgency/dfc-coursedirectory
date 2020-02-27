using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByUkprnHandler : ICosmosDbQueryHandler<GetProviderByUkprn, Provider>
    {
        public Provider Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderByUkprn request) =>
            inMemoryDocumentStore.Providers.All
                .SingleOrDefault(p => p.UnitedKingdomProviderReferenceNumber == request.Ukprn.ToString());
    }
}
