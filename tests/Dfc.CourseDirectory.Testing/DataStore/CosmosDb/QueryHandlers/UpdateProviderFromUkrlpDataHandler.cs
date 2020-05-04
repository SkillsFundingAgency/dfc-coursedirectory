using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<UpdateProviderFromUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderFromUkrlpData request)
        {
            var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.Id);

            if (provider != null)
            {
                provider.ProviderName = request.ProviderName;
                provider.ProviderContact = request.ProviderContact;
                provider.Alias = request.Alias;
                provider.ProviderStatus = request.ProviderStatus;
                provider.DateUpdated = request.DateUpdated;
                provider.UpdatedBy = request.UpdatedBy;

                inMemoryDocumentStore.Providers.Save(provider);
            }


            return new Success();
        }
    }
}
