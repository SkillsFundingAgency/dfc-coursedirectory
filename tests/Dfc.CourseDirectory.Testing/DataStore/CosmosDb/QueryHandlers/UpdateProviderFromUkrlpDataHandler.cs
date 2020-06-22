using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<UpdateProviderFromUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderFromUkrlpData request)
        {
            var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.ProviderId);

            if (provider != null)
            {
                provider.ProviderName = request.ProviderName;
                provider.ProviderAliases = request.Aliases.ToList();
                provider.ProviderContact = request.Contacts.ToList();
                provider.Alias = request.Alias;
                provider.ProviderStatus = request.ProviderStatus;
                provider.DateUpdated = request.DateUpdated;
                provider.UpdatedBy = request.UpdatedBy;

                inMemoryDocumentStore.Providers.Save(provider);
            }


            return new Success();
        }
    }
}
