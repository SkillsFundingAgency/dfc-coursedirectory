using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForCourseHandler : ICosmosDbQueryHandler<GetProviderUkprnForCourse, int?>
    {
        public async Task<int?> Execute(
            DocumentClient client,
            Configuration configuration,
            GetProviderUkprnForCourse request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName);

            var query = new SqlQuerySpec(
                "SELECT c.ProviderUKPRN FROM c WHERE c.id = @CourseId AND c.CourseStatus NOT IN (4, 8)",
                new SqlParameterCollection()
                {
                    new SqlParameter("@CourseId", request.CourseId)
                });

            var response = await client.CreateDocumentQuery<Course>(collectionUri, query, new FeedOptions() { EnableCrossPartitionQuery = true })
                .AsDocumentQuery()
                .ExecuteNextAsync<Course>();

            if (response.Count == 0)
            {
                return null;
            }
            else
            {
                return response.Single().ProviderUKPRN;
            }
        }
    }
}
