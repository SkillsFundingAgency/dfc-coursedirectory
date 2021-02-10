using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderOnboardedHandler : ICosmosDbQueryHandler<UpdateProviderOnboarded, OneOf<NotFound, AlreadyOnboarded, Success>>
    {
        public async Task<OneOf<NotFound, AlreadyOnboarded, Success>> Execute(DocumentClient client, Configuration configuration, UpdateProviderOnboarded request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            try
            {
                var response = await client.ReadDocumentAsync<Provider>(documentUri);

                var provider = response.Document;

                if (provider.Status == ProviderStatus.Onboarded)
                {
                    return new AlreadyOnboarded();
                }

                provider.Status = ProviderStatus.Onboarded;
                provider.DateOnboarded = request.UpdatedDateTime;
                provider.DateUpdated = request.UpdatedDateTime;
                provider.UpdatedBy = request.UpdatedBy.UserId;

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
