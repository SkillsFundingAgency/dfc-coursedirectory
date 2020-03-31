using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderUkrlpDataHandler : ICosmosDbQueryHandler<UpdateProviderUkrlpData, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderUkrlpData request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            var response = await client.ReadDocumentAsync<Provider>(documentUri);

            var provider = response.Document;

            provider.DateUpdated = request.UpdatedOn;
            request.UpdatedBy.Switch(_ => { }, v => provider.UpdatedBy = v);

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}
