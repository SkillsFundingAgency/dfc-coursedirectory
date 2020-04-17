using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderInfoHandler : ICosmosDbQueryHandler<UpdateProviderInfo, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateProviderInfo request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName,
                request.ProviderId.ToString());

            var response = await client.ReadDocumentAsync<Provider>(documentUri);

            var provider = response.Document;

            request.Alias.Switch(_ => { }, v => provider.Alias = v);
            request.MarketingInformation.Switch(_ => { }, v => provider.MarketingInformation = v);
            request.CourseDirectoryName.Switch(_ => { }, v => provider.CourseDirectoryName = v);
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;

            await client.ReplaceDocumentAsync(documentUri, provider);

            return new Success();
        }
    }
}
