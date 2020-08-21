using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForApprenticeshipHandler
        : ICosmosDbQueryHandler<GetProviderUkprnForApprenticeship, int?>
    {
        public int? Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            GetProviderUkprnForApprenticeship request)
        {
            var doc = inMemoryDocumentStore.Apprenticeships.All
                .SingleOrDefault(a => a.Id == request.ApprenticeshipId &&
                    a.RecordStatus != 4 &&
                    a.RecordStatus != 8);

            return doc?.ProviderUKPRN;
        }
    }
}
