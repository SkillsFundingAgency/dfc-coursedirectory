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
    public class InsertProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<InsertProviderFromUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, InsertProviderFromUkrlpData request)
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

            return new Success();
        }
    }
}
