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
    public class CreateProviderFromUkrlpDataHandler : ICosmosDbQueryHandler<CreateProviderFromUkrlpData, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateProviderFromUkrlpData request)
        {
            var newProvider = new Provider()
            {
                Id = request.Id,
                UnitedKingdomProviderReferenceNumber = request.UnitedKingdomProviderReferenceNumber,
                ProviderName = request.ProviderName,
                ProviderContact = request.ProviderContact,
                Alias = request.Alias,
                ProviderStatus = request.ProviderStatus,
                ProviderType = request.ProviderType,
                DateUpdated = request.DateUpdated,
                UpdatedBy = request.UpdatedBy,
            };

            inMemoryDocumentStore.Providers.Save(newProvider);

            return new Success();
        }
    }
}
