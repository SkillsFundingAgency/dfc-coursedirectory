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
    public class UpsertProviderUkrlpDataHandler : ICosmosDbQueryHandler<UpsertProviderUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertProviderUkrlpData request)
        {
            if (request.Update)
            {
                var provider = inMemoryDocumentStore.Providers.All.SingleOrDefault(p => p.Id == request.Id);

                if (provider != null)
                {
                    provider.ProviderName = request.ProviderName;
                    // Only update if supplied
                    provider.ProviderContact = request.ProviderContact.Count > 0 ? request.ProviderContact : provider.ProviderContact;
                    provider.Alias = request.Alias;
                    provider.ProviderStatus = request.ProviderStatus;
                    provider.DateUpdated = request.DateUpdated;
                    provider.UpdatedBy = request.UpdatedBy;

                    inMemoryDocumentStore.Providers.Save(provider);
                }
            }
            else
            {
                var newProvider = new Provider();
                newProvider.Id = request.Id;
                newProvider.UnitedKingdomProviderReferenceNumber = request.UnitedKingdomProviderReferenceNumber;
                newProvider.ProviderName = request.ProviderName;
                newProvider.ProviderContact = request.ProviderContact;
                newProvider.Alias = request.Alias;
                newProvider.ProviderStatus = request.ProviderStatus;
                newProvider.ProviderType = request.ProviderType; //Default to FE
                newProvider.DateUpdated = request.DateUpdated;
                newProvider.UpdatedBy = request.UpdatedBy;

                inMemoryDocumentStore.Providers.Save(newProvider);
            }

            return new Success();
        }
    }
}
