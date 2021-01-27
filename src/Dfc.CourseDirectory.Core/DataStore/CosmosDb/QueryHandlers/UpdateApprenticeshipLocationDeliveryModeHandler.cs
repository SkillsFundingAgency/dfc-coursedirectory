using System.Linq;
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
    public class UpdateApprenticeshipLocationDeliveryModeHandler : ICosmosDbQueryHandler<UpdateApprenticeshipLocationDeliveryMode, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(DocumentClient client, Configuration configuration, UpdateApprenticeshipLocationDeliveryMode request)
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
            catch (DocumentClientException dex)
                when (dex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            var location = apprenticeship.ApprenticeshipLocations.SingleOrDefault(l => l.Id == request.ApprenticeshipLocationId);

            if (location == null)
            {
                return new NotFound();
            }

            location.DeliveryModes = request.DeliveryModes.ToList();

            await client.ReplaceDocumentAsync(
                documentUri,
                apprenticeship,
                new RequestOptions() { PartitionKey = partitionKey });

            return new Success();
        }
    }
}
