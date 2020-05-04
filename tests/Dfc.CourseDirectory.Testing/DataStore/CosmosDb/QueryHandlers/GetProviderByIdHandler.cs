using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByIdHandler : ICosmosDbQueryHandler<GetProviderById, Provider>
    {
        public Provider Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderById request) =>
            inMemoryDocumentStore.Providers.All
                .SingleOrDefault(p => p.Id == request.ProviderId);
    }
}
