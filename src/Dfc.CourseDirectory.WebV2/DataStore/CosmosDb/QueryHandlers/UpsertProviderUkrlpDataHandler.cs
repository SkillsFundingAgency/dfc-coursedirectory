using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertProviderUkrlpDataHandler : ICosmosDbQueryHandler<UpsertProviderUkrlpData, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            UpsertProviderUkrlpData request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.Id.ToString());

            if (request.Update)
            {
                var response = await client.ReadDocumentAsync<Provider>(documentUri);

                var provider = response.Document;

                if (provider != null)
                {
                    provider.ProviderName = request.ProviderName;
                    // Only update if supplied
                    provider.ProviderContact = request.ProviderContact.Count > 0 ? request.ProviderContact : provider.ProviderContact;
                    provider.Alias = request.Alias;
                    provider.ProviderStatus = request.ProviderStatus;
                    provider.DateUpdated = request.DateUpdated;
                    request.UpdatedBy = request.UpdatedBy;

                    await client.ReplaceDocumentAsync(documentUri, provider);
                }
            }
            else
            {
                var newProvider = new Provider();
                newProvider.Id = request.Id;
                newProvider.UnitedKingdomProviderReferenceNumber = request.UnitedKingdomProviderReferenceNumber;
                newProvider.ProviderName = request.ProviderName;
                newProvider.ProviderContact = request.ProviderContact;
                newProvider.Alias = request.Alias;
                newProvider.ProviderStatus = request.ProviderStatus;
                newProvider.ProviderType = WebV2.Models.ProviderType.FE; //Default to FE
                newProvider.DateUpdated = request.DateUpdated;
                newProvider.UpdatedBy = request.UpdatedBy;

                await client.CreateDocumentAsync(documentUri, newProvider);
            }

            return new Success();
        }
    }
}
