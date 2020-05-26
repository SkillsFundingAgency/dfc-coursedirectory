using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ArchiveCoursesForProviderHandler : ICosmosDbQueryHandler<ArchiveCoursesForProvider, int>
    {
        public async Task<int> Execute(DocumentClient client, Configuration configuration, ArchiveCoursesForProvider request)
        {
            var sprocUri = UriFactory.CreateStoredProcedureUri(
                configuration.DatabaseId,
                configuration.CoursesCollectionName,
                "ArchiveCoursesForProvider");

            var result = await client.ExecuteStoredProcedureAsync<StoredProcResponse>(
                sprocUri,
                new RequestOptions()
                {
                    PartitionKey = new Microsoft.Azure.Documents.PartitionKey(request.Ukprn)
                },
                request.Ukprn);

            return result.Response.Updated;
        }

        private class StoredProcResponse
        {
            public int Updated { get; set; }
        }
    }
}
