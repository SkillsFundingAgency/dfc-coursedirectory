using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class CreateFrameworkHandler : ICosmosDbQueryHandler<CreateFramework, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateFramework request)
        {
            var framework = new Framework()
            {
                Id = request.Id,
                FrameworkCode = request.FrameworkCode,
                ProgType = request.ProgType,
                PathwayCode = request.PathwayCode,
                NasTitle = request.NasTitle,
                RecordStatusId = 2
            };
            inMemoryDocumentStore.Frameworks.Save(framework);

            return new Success();
        }
    }
}
