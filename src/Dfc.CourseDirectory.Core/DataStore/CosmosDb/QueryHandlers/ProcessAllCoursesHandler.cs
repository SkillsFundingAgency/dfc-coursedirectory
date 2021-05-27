using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ProcessAllCoursesHandler : ICosmosDbQueryHandler<ProcessAllCourses, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, ProcessAllCourses request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName);

            var query = client.CreateDocumentQuery<Course>(
                collectionUri,
                new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 }).AsQueryable();

            if (request.Filter != null)
            {
                query = query.Where(request.Filter);
            }

            await query.AsDocumentQuery().ProcessAll(request.ProcessChunk);

            return new None();
        }
    }
}
