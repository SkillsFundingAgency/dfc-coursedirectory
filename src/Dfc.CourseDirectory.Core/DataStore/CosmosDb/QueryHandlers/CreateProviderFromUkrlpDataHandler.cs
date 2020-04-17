using System;
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
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.Id.ToString());

            var newProvider = new Provider()
            {
                Id = request.Id,
                UnitedKingdomProviderReferenceNumber = request.UnitedKingdomProviderReferenceNumber,
                ProviderName = request.ProviderName,
                ProviderContact = request.ProviderContact,
                Alias = request.Alias,
                ProviderStatus = request.ProviderStatus,
                ProviderType = request.ProviderType,
                DateUpdated = request.DateUpdated,
                UpdatedBy = request.UpdatedBy,
            };

            await client.CreateDocumentAsync(documentUri, newProvider);

            return new Success();
        }
    }
}
