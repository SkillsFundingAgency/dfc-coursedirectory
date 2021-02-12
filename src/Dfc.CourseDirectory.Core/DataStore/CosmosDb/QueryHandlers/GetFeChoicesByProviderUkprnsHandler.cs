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
    public class GetFeChoicesByProviderUkprnsHandler : ICosmosDbQueryHandler<GetFeChoicesByProviderUkprns, IReadOnlyDictionary<int, FeChoice>>
    {
        public async Task<IReadOnlyDictionary<int, FeChoice>> Execute(DocumentClient client, Configuration configuration, GetFeChoicesByProviderUkprns request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.FeChoicesCollectionName);

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = -1 };

            var results = await client.CreateDocumentQuery<FeChoice>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(f => request.ProviderUkprns.Contains(f.UKPRN))
                .AsDocumentQuery()
                .FetchAll();

            return results.ToDictionary(f => f.UKPRN, f => f);
        }
    }
}
