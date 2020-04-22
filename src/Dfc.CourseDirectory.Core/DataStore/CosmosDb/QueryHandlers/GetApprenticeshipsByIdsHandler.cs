using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var missingApprenticeships = request.ApprenticeshipIds.Where(id => !resultsDict.ContainsKey(id)).ToList();
            if (missingApprenticeships.Count > 0)
            {
                var exceptions = missingApprenticeships.Select(id => new ResourceDoesNotExistException(ResourceType.Apprenticeship, id));

                if (missingApprenticeships.Count == 1)
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
