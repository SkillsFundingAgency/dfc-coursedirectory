using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetAllCoursesForProviderHandler :
        ICosmosDbQueryHandler<GetAllCoursesForProvider, IReadOnlyCollection<Course>>
    {
        public async Task<IReadOnlyCollection<Course>> Execute(
            DocumentClient client,
            Configuration configuration,
            GetAllCoursesForProvider request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName);

            var query = client.CreateDocumentQuery<Course>(
                    collectionUri,
                    new SqlQuerySpec()
                    {
                        QueryText = "select * from c where (c.CourseStatus & @CourseStatuses) <> 0 and c.ProviderUKPRN = @ProviderUkprn",
                        Parameters = new SqlParameterCollection()
                        {
                            new SqlParameter("@CourseStatuses", (int)request.CourseStatuses),
                            new SqlParameter("@ProviderUkprn", request.ProviderUkprn)
                        }
                    },
                    new FeedOptions()
                    {
                        PartitionKey = new PartitionKey(request.ProviderUkprn),
                        MaxItemCount = -1
                    })
                .AsDocumentQuery();

            return await query.FetchAll();
        }
    }
}
