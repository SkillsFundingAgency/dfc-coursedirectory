using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByIdHandler : ICosmosDbQueryHandler<GetProviderById, Provider>
    {
        public Provider Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderById request) =>
            inMemoryDocumentStore.Providers.All
                .SingleOrDefault(p => p.Id == request.ProviderId);
    }
}
