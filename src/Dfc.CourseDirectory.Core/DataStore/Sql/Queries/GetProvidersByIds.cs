using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProvidersByIds : ISqlQuery<IReadOnlyDictionary<Guid, Provider>>
    {
        public IEnumerable<Guid> ProviderIds { get; set; }
    }
}
