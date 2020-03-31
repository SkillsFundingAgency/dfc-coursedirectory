using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllStandardsQueryHandler :
        ICosmosDbQueryHandler<GetAllStandards, IReadOnlyCollection<Standard>>
    {
        public IReadOnlyCollection<Standard> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            GetAllStandards request)
        {
            return inMemoryDocumentStore.Standards.All;
        }
    }
}
