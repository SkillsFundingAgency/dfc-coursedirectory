using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetProvidersByIdsHandler : ICosmosDbQueryHandler<GetProvidersByIds, IDictionary<Guid, Provider>>
    {
        public IDictionary<Guid, Provider> Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProvidersByIds request) =>
            inMemoryDocumentStore.Providers.All
                .Where(p => request.ProviderIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);
    }
}
