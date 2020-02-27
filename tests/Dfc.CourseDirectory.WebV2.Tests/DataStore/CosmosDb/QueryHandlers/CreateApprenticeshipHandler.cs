using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class CreateApprenticeshipHandler : ICosmosDbQueryHandler<CreateApprenticeship, CreateApprenticeshipStatus>
    {
        public CreateApprenticeshipStatus Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateApprenticeship request)
        {
            var apprenticeship = new Apprenticeship()
            {
                Id = request.ApprenticeshipId,
                ProviderUKPRN = request.ProviderUkprn
            };
            inMemoryDocumentStore.Apprenticeships.Save(apprenticeship);

            return CreateApprenticeshipStatus.Ok;
        }
    }
}
