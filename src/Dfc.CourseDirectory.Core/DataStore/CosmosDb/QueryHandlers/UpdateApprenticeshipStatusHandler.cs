using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipStatusHandler :
        ICosmosDbQueryHandler<UpdateApprenticeshipStatus, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateApprenticeshipStatus request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName,
                request.ApprenticeshipId.ToString());

            var partitionKey = new PartitionKey(request.ProviderUkprn);

            Apprenticeship apprenticeship;

            try
            {
                var query = await client.ReadDocumentAsync<Apprenticeship>(
                    documentUri,
                    new RequestOptions() { PartitionKey = partitionKey });

                apprenticeship = query.Document;
            }
            catch (DocumentClientException dex) when (dex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            apprenticeship.RecordStatus = request.Status;

            await client.ReplaceDocumentAsync(
                documentUri,
                apprenticeship,
                new RequestOptions() { PartitionKey = partitionKey });

            return new Success();
        }
    }
}
