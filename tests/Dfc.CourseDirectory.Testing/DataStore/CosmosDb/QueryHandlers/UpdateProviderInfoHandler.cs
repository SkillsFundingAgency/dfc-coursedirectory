using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderInfoHandler : ICosmosDbQueryHandler<UpdateProviderInfo, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderInfo request)
        {
            var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.ProviderId);

            provider.MarketingInformation = request.MarketingInformation;
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;

            inMemoryDocumentStore.Providers.Save(provider);

            return new Success();
        }
    }
}
