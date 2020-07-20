using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ProcessAllVenuesHandler : ICosmosDbQueryHandler<ProcessAllVenues, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, ProcessAllVenues request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.VenuesCollectionName);

            var query = client.CreateDocumentQuery<Venue>(
                    collectionUri,
                    "select * from c where c.Latitude <> null and c.Longitude <> null",
                    new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 })
                .AsDocumentQuery();

            await query.ProcessAll(request.ProcessChunk);

            return new None();
        }
    }
}
