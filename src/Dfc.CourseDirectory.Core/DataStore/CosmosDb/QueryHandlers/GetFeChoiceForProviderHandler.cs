using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetFeChoiceForProviderHandler : ICosmosDbQueryHandler<GetFeChoiceForProvider, FeChoice>
    {
        public async Task<FeChoice> Execute(DocumentClient client, Configuration configuration, GetFeChoiceForProvider request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.FeChoicesCollectionName);

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = -1 };

            var response = await client.CreateDocumentQuery<FeChoice>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(f => f.UKPRN == request.ProviderUkprn)
                .AsDocumentQuery()
                .ExecuteNextAsync();

            return response.SingleOrDefault();
        }
    }
}
