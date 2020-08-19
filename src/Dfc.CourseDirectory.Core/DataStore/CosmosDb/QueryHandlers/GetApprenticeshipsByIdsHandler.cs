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
    public class GetApprenticeshipsByIdsHandler : ICosmosDbQueryHandler<GetApprenticeshipsByIds, IDictionary<Guid, Apprenticeship>>
    {
        public async Task<IDictionary<Guid, Apprenticeship>> Execute(DocumentClient client, Configuration configuration, GetApprenticeshipsByIds request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                 configuration.DatabaseId,
                 configuration.ApprenticeshipCollectionName);

            var query = client.CreateDocumentQuery<Apprenticeship>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => request.ApprenticeshipIds.Contains(p.Id))
                .AsDocumentQuery();

            var results = await query.FetchAll();
            var resultsDict = results.ToDictionary(r => r.Id, r => r);

            return resultsDict;
        }
    }
}
