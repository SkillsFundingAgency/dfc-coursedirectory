using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderInfoHandler : ICosmosDbQueryHandler<UpdateProviderInfo, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderInfo request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            Provider provider;

            try
            {
                var response = await client.ReadDocumentAsync<Provider>(documentUri);

                provider = response.Document;
            }
            catch (DocumentClientException dex) when (dex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            provider.MarketingInformation = request.MarketingInformation;
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}
