using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByIdHandler : ICosmosDbQueryHandler<GetProviderById, Provider>
    {
        public async Task<Provider> Execute(DocumentClient client, Configuration configuration, GetProviderById request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            var response = await client.ReadDocumentAsync<Provider>(documentUri);

            return response.Document;
        }
    }
}
