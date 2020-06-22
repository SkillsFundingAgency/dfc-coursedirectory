using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class CreateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<CreateProviderFromUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateProviderFromUkrlpData request)
        {
            var newProvider = new Provider()
            {
                Id = request.ProviderId,
                UnitedKingdomProviderReferenceNumber = request.Ukprn.ToString(),
                ProviderName = request.ProviderName,
                ProviderAliases = request.Aliases.ToList(),
                ProviderContact = request.Contacts.ToList(),
                Alias = request.Alias,
                ProviderStatus = request.ProviderStatus,
                ProviderType = request.ProviderType,
                DateUpdated = request.DateUpdated,
                UpdatedBy = request.UpdatedBy,
            };

            inMemoryDocumentStore.Providers.Save(newProvider);

            return new Success();
        }
    }
}
