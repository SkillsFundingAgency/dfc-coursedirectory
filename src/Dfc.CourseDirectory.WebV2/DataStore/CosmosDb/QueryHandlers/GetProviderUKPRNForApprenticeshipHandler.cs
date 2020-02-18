using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForApprenticeshipHandler : ICosmosDbQueryHandler<GetProviderUkprnForApprenticeship, int?>
    {
        public async Task<int?> Execute(
            DocumentClient client,
            Configuration configuration,
            GetProviderUkprnForApprenticeship request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName);

            var query = new SqlQuerySpec(
                "SELECT c.ProviderUKPRN FROM c WHERE c.id = @apprenticeshipId",
                new SqlParameterCollection()
                {
                    new SqlParameter("@apprenticeshipId", request.ApprenticeshipId)
                });

            var response = await client.CreateDocumentQuery(collectionUri, query, new FeedOptions() { EnableCrossPartitionQuery = true })
                .AsDocumentQuery()
                .ExecuteNextAsync();

            return response.Count > 0 ? (int?)response.Single().ProviderUKPRN : null;
        }
    }
}
