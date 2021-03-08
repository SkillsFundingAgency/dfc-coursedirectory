using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class DeleteApprenticeshipHandler : ICosmosDbQueryHandler<Queries.DeleteApprenticeship, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(DocumentClient client, Configuration configuration, Queries.DeleteApprenticeship request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName,
                request.ApprenticeshipId.ToString());

            try
            {
                var result = await client.DeleteDocumentAsync(documentUri, new RequestOptions { PartitionKey = new PartitionKey(request.ProviderUkprn) });

                return new Success();
            }
            catch (DocumentClientException ex)
                when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }
        }
    }
}
