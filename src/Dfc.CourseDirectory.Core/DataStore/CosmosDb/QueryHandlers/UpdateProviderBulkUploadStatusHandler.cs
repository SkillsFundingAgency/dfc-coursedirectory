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
    public class UpdateProviderBulkUploadStatusHandler : ICosmosDbQueryHandler<UpdateProviderBulkUploadStatus, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(DocumentClient client, Configuration configuration, UpdateProviderBulkUploadStatus request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            try
            {
                var response = await client.ReadDocumentAsync<Provider>(documentUri);

                var provider = response.Document;

                if (provider.BulkUploadStatus == null)
                {
                    provider.BulkUploadStatus = new ProviderBulkUploadStatus();
                }

                provider.BulkUploadStatus.PublishInProgress = request.PublishInProgress;

                await client.ReplaceDocumentAsync(documentUri, provider);

                return new Success();
            }
            catch (DocumentClientException dex) when (dex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }
        }
    }
}
