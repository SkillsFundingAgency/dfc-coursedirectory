using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ProcessAllFeChoicesHandler : ICosmosDbQueryHandler<ProcessAllFeChoices, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, ProcessAllFeChoices request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.FeChoicesCollectionName);

            var query = client.CreateDocumentQuery<FeChoice>(
                collectionUri,
                new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 }).AsDocumentQuery();

            await query.ProcessAll(request.ProcessChunk);

            return new None();
        }
    }
}
