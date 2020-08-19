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
                .SingleOrDefault(a => a.Id == request.ApprenticeshipId);

            if (doc == null)
            {
                return null;
            }
            else
            {
                return doc.ProviderUKPRN;
            }
        }
    }
}
