using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class CreateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<CreateProviderFromUkrlpData, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            CreateProviderFromUkrlpData request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName);

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
                Status = request.Status,
                DateUpdated = request.DateUpdated,
                UpdatedBy = request.UpdatedBy,
            };

            await client.CreateDocumentAsync(collectionUri, newProvider);

            return new Success();
        }
    }
}
