using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderOnboardedHandler : ICosmosDbQueryHandler<UpdateProviderOnboarded, OneOf<NotFound, AlreadyOnboarded, Success>>
    {
        public OneOf<NotFound, AlreadyOnboarded, Success> Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderOnboarded request)
        {
            var provider = inMemoryDocumentStore.Providers.All.FirstOrDefault(p => p.Id == request.ProviderId);

            if (provider == null)
            {
                return new NotFound();
            }

            if (provider.Status == ProviderStatus.Onboarded)
            {
                return new AlreadyOnboarded();
            }

            provider.Status = ProviderStatus.Onboarded;
            provider.DateOnboarded = request.UpdatedDateTime;
            provider.DateUpdated = request.UpdatedDateTime;
            provider.UpdatedBy = request.UpdatedBy.UserId;

            inMemoryDocumentStore.Providers.Save(provider);

            return new Success();
        }
    }
}
