using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetCourseMigrationReportForProviderHandler : ICosmosDbQueryHandler<GetCourseMigrationReportForProvider, CourseMigrationReport>
    {
        public async Task<CourseMigrationReport> Execute(DocumentClient client, Configuration configuration, GetCourseMigrationReportForProvider request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName);

            var response = await client.CreateDocumentQuery<CourseMigrationReport>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => p.ProviderUKPRN == request.ProviderUkprn)
                .AsDocumentQuery()
                .ExecuteNextAsync();

            return response.FirstOrDefault();
        }
    }
}
