using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetVenueByIdHandler : ICosmosDbQueryHandler<GetVenueById, Venue>
    {
        public async Task<Venue> Execute(DocumentClient client, Configuration configuration, GetVenueById request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.VenuesCollectionName);

            var query = client.CreateDocumentQuery<Venue>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(v => v.Status != (int)VenueStatus.Archived)
                .Where(v => v.Id == request.VenueId)
                .AsDocumentQuery();

            return (await query.ExecuteNextAsync()).SingleOrDefault();
        }
    }
}
