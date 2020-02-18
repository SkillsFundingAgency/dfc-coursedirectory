using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForApprenticeshipHandler : ICosmosDbQueryHandler<GetProviderUkprnForApprenticeship, int?>
    {
        public int? Execute(InMemoryDocumentStore inMemoryDocumentStore, GetProviderUkprnForApprenticeship request) =>
            inMemoryDocumentStore.Apprenticeships.All
                .SingleOrDefault(a => a.Id == request.ApprenticeshipId)
                ?.ProviderUKPRN;
    }
}
