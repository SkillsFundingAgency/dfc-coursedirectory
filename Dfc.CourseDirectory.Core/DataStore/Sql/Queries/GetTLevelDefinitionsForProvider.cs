using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetTLevelDefinitionsForProvider : ISqlQuery<IReadOnlyCollection<TLevelDefinition>>
    {
        public Guid ProviderId { get; set; }
    }
}
