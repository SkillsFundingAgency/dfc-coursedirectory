using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class InsertProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<InsertProviderFromUkrlpData, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            InsertProviderFromUkrlpData request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.Id.ToString());

            var newProvider = new Provider();
            newProvider.Id = request.Id;
            newProvider.UnitedKingdomProviderReferenceNumber = request.UnitedKingdomProviderReferenceNumber;
            newProvider.ProviderName = request.ProviderName;
            newProvider.ProviderContact = request.ProviderContact;
            newProvider.Alias = request.Alias;
            newProvider.ProviderStatus = request.ProviderStatus;
            newProvider.ProviderType = request.ProviderType;
            newProvider.DateUpdated = request.DateUpdated;
            newProvider.UpdatedBy = request.UpdatedBy;

            await client.CreateDocumentAsync(documentUri, newProvider);

            return new Success();
        }
    }
}
