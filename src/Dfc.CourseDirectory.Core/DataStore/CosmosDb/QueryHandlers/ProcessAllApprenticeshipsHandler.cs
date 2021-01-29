using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ProcessAllApprenticeshipsHandler : ICosmosDbQueryHandler<ProcessAllApprenticeships, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, ProcessAllApprenticeships request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName);

            var query = client.CreateDocumentQuery<Apprenticeship>(
                collectionUri,
                new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 })
                .AsQueryable();

            if (request.Predicate != null)
            {
                query = query.Where(request.Predicate);
            }

            await query
                .AsDocumentQuery()
                .ProcessAll(request.ProcessChunk);

            return new None();
        }
    }
}
