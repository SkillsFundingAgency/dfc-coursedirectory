using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetProvidersByIdsHandler : ICosmosDbQueryHandler<GetProvidersByIds, IDictionary<Guid, Provider>>
    {
        public async Task<IDictionary<Guid, Provider>> Execute(
            DocumentClient client,
            Configuration configuration,
            GetProvidersByIds request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName);

            var query = client.CreateDocumentQuery<Provider>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => request.ProviderIds.Contains(p.Id))
                .AsDocumentQuery();

            var results = await query.FetchAll();
            var resultsDict = results.ToDictionary(r => r.Id, r => r);

            return resultsDict;
        }
    }
}
