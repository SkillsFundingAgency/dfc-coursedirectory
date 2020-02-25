using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderInfoHandler : ICosmosDbQueryHandler<UpdateProviderInfo, OneOf<Success, NotFound>>
    {
        public OneOf<Success, NotFound> Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderInfo request)
        {
            var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.ProviderId);
            
            if (provider == null)
            {
                return new NotFound();
            }

            provider.Alias = request.Alias;
            provider.DateUpdated = request.UpdatedOn;
            provider.UpdatedBy = request.UpdatedBy.Email;

            request.BriefOverview.Switch(v => provider.MarketingInformation = v, _ => { });
            request.CourseDirectoryName.Switch(v => provider.CourseDirectoryName = v, _ => { });

            inMemoryDocumentStore.Providers.Save(provider);

            return new Success();
        }
    }
}
