using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetAuthorizedTLevelDefinitionsForProvider : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }

        public IEnumerable<Guid> TLevelDefinitionIds { get; set; }
    }
}
