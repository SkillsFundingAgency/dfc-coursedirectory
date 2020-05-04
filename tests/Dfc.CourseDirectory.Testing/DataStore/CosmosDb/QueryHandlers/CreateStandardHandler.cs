using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class CreateStandardHandler : ICosmosDbQueryHandler<CreateStandard, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateStandard request)
        {
            var standard = new Standard()
            {
                Id = request.Id,
                StandardCode = request.StandardCode,
                Version = request.Version,
                StandardName = request.StandardName,
                NotionalEndLevel = request.NotionalEndLevel,
                OtherBodyApprovalRequired = request.OtherBodyApprovalRequired,
                RecordStatusId = 2
            };
            inMemoryDocumentStore.Standards.Save(standard);

            return new Success();
        }
    }
}
