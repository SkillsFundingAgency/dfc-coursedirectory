using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderMarketingInfoHandler : ICosmosDbQueryHandler<UpdateProviderMarketingInfo, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderMarketingInfo request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            var response = await client.ReadDocumentAsync<Provider>(documentUri);

            var provider = response.Document;

            provider.MarketingInformation = request.MarketingInformation;
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}