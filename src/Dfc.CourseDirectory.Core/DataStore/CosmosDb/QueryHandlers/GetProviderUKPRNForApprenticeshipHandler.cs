using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderUkprnForApprenticeshipHandler
        : ICosmosDbQueryHandler<GetProviderUkprnForApprenticeship, int?>
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
                "SELECT c.ProviderUKPRN FROM c WHERE c.id = @apprenticeshipId AND c.RecordStatus NOT IN (4, 8)",
                new SqlParameterCollection()
                {
                    new SqlParameter("@apprenticeshipId", request.ApprenticeshipId)
                });

            var response = await client.CreateDocumentQuery<Apprenticeship>(collectionUri, query, new FeedOptions() { EnableCrossPartitionQuery = true })
                .AsDocumentQuery()
                .ExecuteNextAsync<Apprenticeship>();

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
