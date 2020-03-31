using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllFrameworksQueryHandler :
        ICosmosDbQueryHandler<GetAllFrameworks, IReadOnlyCollection<Framework>>
    {
        public IReadOnlyCollection<Framework> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            GetAllFrameworks request)
        {
            return inMemoryDocumentStore.Frameworks.All;
        }
    }
}
