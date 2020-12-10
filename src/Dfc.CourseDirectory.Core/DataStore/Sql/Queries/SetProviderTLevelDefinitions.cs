using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderTLevelDefinitions :
        ISqlQuery<(IReadOnlyCollection<Guid> Added, IReadOnlyCollection<Guid> Removed)>
    {
        public Guid ProviderId { get; set; }

        public IEnumerable<Guid> TLevelDefinitionIds { get; set; }
    }
}
