using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderNonLarsSubTypes :
        ISqlQuery<(IReadOnlyCollection<Guid> Added, IReadOnlyCollection<Guid> Removed)>
    {
        public Guid ProviderId { get; set; }

        public IEnumerable<Guid> NonLarsSubTypeIds { get; set; }
    }
}
