using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries
{
    public class CreateProvider : ICosmosDbQuery<CreateProviderResult>
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public string ProviderName { get; set; }
    }

    public enum CreateProviderResult { Ok }
}
