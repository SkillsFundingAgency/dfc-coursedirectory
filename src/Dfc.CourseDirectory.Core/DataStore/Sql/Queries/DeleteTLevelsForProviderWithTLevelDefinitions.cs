using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteTLevelsForProviderWithTLevelDefinitions : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }
        public IEnumerable<Guid> TLevelDefinitionIds { get; set; }
        public UserInfo DeletedBy { get; set; }
        public DateTime DeletedOn { get; set; }
    }
}
