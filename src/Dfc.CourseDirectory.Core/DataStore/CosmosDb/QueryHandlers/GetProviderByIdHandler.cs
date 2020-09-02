using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByIdHandler : ICosmosDbQueryHandler<GetProviderById, Provider>
    {
        public async Task<Provider> Execute(DocumentClient client, Configuration configuration, GetProviderById request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            try
            {
                var response = await client.ReadDocumentAsync<Provider>(documentUri);
                return response.Document;
            }
            catch (DocumentClientException dex) when (dex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
