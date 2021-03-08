using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetApprenticeshipByIdHandler : ICosmosDbQueryHandler<GetApprenticeshipById, Apprenticeship>
    {
        public async Task<Apprenticeship> Execute(DocumentClient client, Configuration configuration, GetApprenticeshipById request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName);

            var result = await client.CreateDocumentQuery<Apprenticeship>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(a => a.Id == request.ApprenticeshipId)
                .Where(p => p.RecordStatus != (int)ApprenticeshipStatus.Archived && p.RecordStatus != (int)ApprenticeshipStatus.Deleted)
                .AsDocumentQuery()
                .ExecuteNextAsync();

            return result.SingleOrDefault();
        }
    }
}
