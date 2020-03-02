using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetProviderById : ICosmosDbQuery<Provider>
    {
        public Guid ProviderId { get; set; }
    }
}
