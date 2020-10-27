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
    public class UpdateProviderTypeHandler : ICosmosDbQueryHandler<UpdateProviderType, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderType request)
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

            provider.ProviderType = request.ProviderType;

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}
