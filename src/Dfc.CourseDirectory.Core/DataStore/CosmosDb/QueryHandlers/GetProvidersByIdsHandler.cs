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

            var missingProviders = request.ProviderIds.Where(id => !resultsDict.ContainsKey(id)).ToList();
            if (missingProviders.Count > 0)
            {
                var exceptions = missingProviders.Select(id => new ResourceDoesNotExistException(ResourceType.Provider, id));

                if (missingProviders.Count == 1)
                {
                    throw exceptions.Single();
                }
                else
                {
                    throw new AggregateException(exceptions);
                }
            }

            return resultsDict;
        }
    }
}
