using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetVenuesByProviderHandler : ICosmosDbQueryHandler<GetVenuesByProvider, IReadOnlyCollection<Venue>>
    {
        public Task<IReadOnlyCollection<Venue>> Execute(
            DocumentClient client,
            Configuration configuration,
            GetVenuesByProvider request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.VenuesCollectionName);

            return client
                .CreateDocumentQuery<Venue>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                .Where(v => v.Status != (int)VenueStatus.Archived)
                .Where(v => v.Ukprn == request.ProviderUkprn)
                .AsDocumentQuery()
                .FetchAll();
        }
    }
}
