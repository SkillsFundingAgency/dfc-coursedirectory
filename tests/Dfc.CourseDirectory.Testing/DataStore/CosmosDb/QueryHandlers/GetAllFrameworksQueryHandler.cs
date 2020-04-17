using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
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
