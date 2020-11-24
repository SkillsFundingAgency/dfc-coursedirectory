using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetCourseByIdHandler : ICosmosDbQueryHandler<GetCourseById, Course>
    {
        public async Task<Course> Execute(DocumentClient client, Configuration configuration, GetCourseById request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName);

            var response = await client.CreateDocumentQuery<Course>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => p.Id == request.CourseId)
                .AsDocumentQuery()
                .ExecuteNextAsync();

            return response.SingleOrDefault();
        }
    }
}
