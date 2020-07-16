using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class CreateProviderHandler : ICosmosDbQueryHandler<CreateProvider, CreateProviderResult>
    {
        public CreateProviderResult Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateProvider request)
        {
            var provider = new Provider()
            {
                Id = request.ProviderId,
                UnitedKingdomProviderReferenceNumber = request.Ukprn.ToString(),
                ProviderType = request.ProviderType,
                ProviderName = request.ProviderName,
                ProviderStatus = request.ProviderStatus,
                MarketingInformation = request.MarketingInformation,
                CourseDirectoryName = request.CourseDirectoryName,
                Alias = request.Alias,
                ProviderContact = request.ProviderContact?.ToList() ?? new List<ProviderContact>(),
                Status = Core.Models.ProviderStatus.Onboarded,
                DateUpdated = request.DateUpdated,
                UpdatedBy = request.UpdatedBy
            };
            inMemoryDocumentStore.Providers.Save(provider);

            return CreateProviderResult.Ok;
        }
    }
}
