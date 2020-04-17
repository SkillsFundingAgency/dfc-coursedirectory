using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
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
