using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderInfoHandler : ICosmosDbQueryHandler<UpdateProviderInfo, OneOf<Success, NotFound>>
    {
        public async Task<OneOf<Success, NotFound>> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderInfo request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName);

            var response = await client.CreateDocumentQuery<Provider>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
                .Where(p => p.Id == request.ProviderId)
                .AsDocumentQuery()
                .ExecuteNextAsync();

            var provider = response.SingleOrDefault();

            if (provider == null)
            {
                return new NotFound();
            }

            provider.Alias = request.Alias;
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;

            request.BriefOverview.Switch(v => provider.MarketingInformation = v, _ => { });
            request.CourseDirectoryName.Switch(v => provider.CourseDirectoryName = v, _ => { });

            await client.UpsertDocumentAsync(collectionUri, provider);

            return new Success();
        }
    }
}
