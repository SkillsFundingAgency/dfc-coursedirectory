using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ReallocateDuplicateApprenticeshipLocationIdsHandler :
        ICosmosDbQueryHandler<ReallocateDuplicateApprenticeshipLocationIds, None>
    {
        public async Task<None> Execute(
            DocumentClient client,
            Configuration configuration,
            ReallocateDuplicateApprenticeshipLocationIds request)
        {
            var locations = request.Apprenticeship.ApprenticeshipLocations;

            foreach (var l in locations)
            {
                if (request.DuplicateLocationIds.Contains(l.Id))
                {
                    l.Id = Guid.NewGuid();
                }
            }

            request.Apprenticeship.UpdatedBy = request.UpdatedBy;
            request.Apprenticeship.UpdatedDate = request.UpdatedOn;

            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName,
                request.Apprenticeship.Id.ToString());

            await client.ReplaceDocumentAsync(
                documentUri,
                request.Apprenticeship,
                new RequestOptions() { PartitionKey = new PartitionKey(request.Apprenticeship.ProviderUKPRN) });

            return new None();
        }
    }
}
