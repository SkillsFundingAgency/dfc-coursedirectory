using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetProvidersByIds : ICosmosDbQuery<IDictionary<Guid, Provider>>
    {
        public IEnumerable<Guid> ProviderIds { get; set; }
    }
}
