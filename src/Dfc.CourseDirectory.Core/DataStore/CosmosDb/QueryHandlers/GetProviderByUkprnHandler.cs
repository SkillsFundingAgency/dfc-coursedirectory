using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByUkprnHandler : ICosmosDbQueryHandler<GetProviderByUkprn, Provider>
    {
        public async Task<Provider> Execute(DocumentClient client, Configuration configuration, GetProviderByUkprn request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName);

            var response = await client.CreateDocumentQuery<Provider>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => p.UnitedKingdomProviderReferenceNumber == request.Ukprn.ToString())
                .AsDocumentQuery()
                .ExecuteNextAsync();

            // FIXME: Once duplicate provider records are removed this should be .SingleOrDefault()
            return response.FirstOrDefault();
        }
    }
}
