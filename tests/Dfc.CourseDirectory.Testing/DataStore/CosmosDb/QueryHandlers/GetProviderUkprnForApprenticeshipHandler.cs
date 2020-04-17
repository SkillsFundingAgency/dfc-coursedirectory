using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForApprenticeshipHandler
        : ICosmosDbQueryHandler<GetProviderUkprnForApprenticeship, OneOf<NotFound, int>>
    {
        public OneOf<NotFound, int> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            GetProviderUkprnForApprenticeship request)
        {
            var doc = inMemoryDocumentStore.Apprenticeships.All
                .SingleOrDefault(a => a.Id == request.ApprenticeshipId);

            if (doc == null)
            {
                return new NotFound();
            }
            else
            {
                return doc.ProviderUKPRN;
            }
        }
    }
}
