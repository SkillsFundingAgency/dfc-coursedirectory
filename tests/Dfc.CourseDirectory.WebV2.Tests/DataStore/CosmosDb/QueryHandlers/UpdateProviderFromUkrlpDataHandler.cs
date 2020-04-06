using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<UpdateProviderFromUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateProviderFromUkrlpData request)
        {
            var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.Id);

            if (provider != null)
            {
                provider.ProviderName = request.ProviderName;
                // Only update if supplied
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
