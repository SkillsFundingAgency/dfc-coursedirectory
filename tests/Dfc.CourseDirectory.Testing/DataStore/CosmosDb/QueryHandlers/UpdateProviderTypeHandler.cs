using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderTypeHandler : ICosmosDbQueryHandler<UpdateProviderType, OneOf<NotFound, Success>>
    {
        public OneOf<NotFound, Success> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            UpdateProviderType request)
        {
            var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.ProviderId);

            if (provider == null)
            {
                return new NotFound();
            }

            provider.ProviderType = request.ProviderType;

            inMemoryDocumentStore.Providers.Save(provider);

            return new Success();
        }
    }
}
