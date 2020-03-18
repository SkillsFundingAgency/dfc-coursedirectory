using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderMarketingInfoHandler : ICosmosDbQueryHandler<UpdateProviderMarketingInfo, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderMarketingInfo request)
        {
            var provider = inMemoryDocumentStore.Providers[request.ProviderId.ToString()];
            provider.MarketingInformation = request.MarketingInformation;
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;
            inMemoryDocumentStore.Providers.Save(provider);

            return new Success();
        }
    }
}
