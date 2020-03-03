using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetProvidersByIds : ICosmosDbQuery<IDictionary<Guid, Provider>>
    {
        public IEnumerable<Guid> ProviderIds { get; set; }
    }
}
