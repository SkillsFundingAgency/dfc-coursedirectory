using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetProviderById : ICosmosDbQuery<Provider>
    {
        public Guid ProviderId { get; set; }
    }
}
