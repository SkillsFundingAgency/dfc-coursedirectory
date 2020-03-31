using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
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
            };
            inMemoryDocumentStore.Providers.Save(provider);

            return CreateProviderResult.Ok;
        }
    }
}
