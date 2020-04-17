using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllFrameworksHandler : ICosmosDbQueryHandler<GetAllFrameworks, IReadOnlyCollection<Framework>>
    {
        public Task<IReadOnlyCollection<Framework>> Execute(
            DocumentClient client,
            Configuration configuration,
            GetAllFrameworks request)
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.FrameworksCollectionName);

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = -1 };

            return client.CreateDocumentQuery<Framework>(collectionLink, feedOptions)
                .Where(f => f.RecordStatusId == 2)
                .AsDocumentQuery()
                .FetchAll();
        }
    }
}
