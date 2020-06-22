using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<UpdateProviderFromUkrlpData, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderFromUkrlpData request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            var response = await client.ReadDocumentAsync<Provider>(documentUri);

            var provider = response.Document;

            provider.ProviderName = request.ProviderName;
            provider.ProviderAliases = request.Aliases.ToList();
            provider.ProviderContact = request.Contacts.ToList();
            provider.Alias = request.Alias;
            provider.ProviderStatus = request.ProviderStatus;
            provider.DateUpdated = request.DateUpdated;
            provider.UpdatedBy = request.UpdatedBy;

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}
