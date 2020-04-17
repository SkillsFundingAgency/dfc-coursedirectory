using System;
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
                request.Id.ToString());

            var response = await client.ReadDocumentAsync<Provider>(documentUri);

            var provider = response.Document;

            provider.ProviderName = request.ProviderName;
            provider.ProviderContact = request.ProviderContact;
            provider.Alias = request.Alias;
            provider.ProviderStatus = request.ProviderStatus;
            provider.DateUpdated = request.DateUpdated;
            provider.UpdatedBy = request.UpdatedBy;

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}
