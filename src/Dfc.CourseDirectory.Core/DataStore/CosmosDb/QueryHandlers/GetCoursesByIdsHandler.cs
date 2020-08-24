using System;
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
    public class GetCoursesByIdsHandler : ICosmosDbQueryHandler<GetCoursesByIds, IDictionary<Guid, Course>>
    {
        public async Task<IDictionary<Guid, Course>> Execute(DocumentClient client, Configuration configuration, GetCoursesByIds request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                 configuration.DatabaseId,
                 configuration.CoursesCollectionName);

            var query = client.CreateDocumentQuery<Course>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => request.CourseIds.Contains(p.Id))
                .Where(p => p.CourseStatus != CourseStatus.Archived && p.CourseStatus != CourseStatus.Deleted)
                .AsDocumentQuery();

            var results = await query.FetchAll();
            var resultsDict = results.ToDictionary(r => r.Id, r => r);

            return resultsDict;
        }
    }
}
