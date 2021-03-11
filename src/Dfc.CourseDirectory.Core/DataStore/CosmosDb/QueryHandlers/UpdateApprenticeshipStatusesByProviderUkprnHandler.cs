using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipStatusesByProviderUkprnHandler : ICosmosDbQueryHandler<UpdateApprenticeshipStatusesByProviderUkprn, Success>
    {
        public async Task<Success> Execute(DocumentClient client, Configuration configuration, UpdateApprenticeshipStatusesByProviderUkprn request)
        {
            var spUri = UriFactory.CreateStoredProcedureUri(configuration.DatabaseId, configuration.ApprenticeshipCollectionName, "UpdateRecordStatuses");

            await client.ExecuteStoredProcedureAsync<dynamic>(
                spUri,
                new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(request.ProviderUkprn), EnableScriptLogging = true },
                request.ProviderUkprn,
                (int)request.CurrentStatus,
                (int)request.NewStatus);

            return new Success();
        }
    }
}
