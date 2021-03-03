using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetCourseTextByLearnAimRefHandler : ICosmosDbQueryHandler<GetCourseTextByLearnAimRef, CourseText>
    {
        public async Task<CourseText> Execute(DocumentClient client, Configuration configuration, GetCourseTextByLearnAimRef request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.CourseTextCollectionName);

            var query = client.CreateDocumentQuery<CourseText>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .AsQueryable();

            if (int.TryParse(request.LearnAimRef, out var learnAimRef))
            {
                query = query.Where(c => (int)c.LearnAimRef == learnAimRef || (string)c.LearnAimRef == request.LearnAimRef);
            }
            else
            {
                query = query.Where(c => (string)c.LearnAimRef == request.LearnAimRef);
            }

            var results = await query
                .AsDocumentQuery()
                .ExecuteNextAsync<CourseText>();

            return results.FirstOrDefault();
        }
    }
}
