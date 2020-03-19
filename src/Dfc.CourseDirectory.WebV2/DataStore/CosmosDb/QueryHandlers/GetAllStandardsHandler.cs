using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllStandardsHandler : ICosmosDbQueryHandler<GetAllStandards, IReadOnlyCollection<Standard>>
    {
        public Task<IReadOnlyCollection<Standard>> Execute(
            DocumentClient client,
            Configuration configuration,
            GetAllStandards request)
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.StandardsCollectionName);

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = -1 };

            return client.CreateDocumentQuery<Standard>(collectionLink, feedOptions)
                .Where(f => f.RecordStatusId == 2)
                .AsDocumentQuery()
                .FetchAll();
        }
    }
}
