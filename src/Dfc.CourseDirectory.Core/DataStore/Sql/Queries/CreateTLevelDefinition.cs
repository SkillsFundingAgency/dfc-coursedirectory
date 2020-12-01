using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateTLevelDefinition : ISqlQuery<Success>
    {
        public Guid TLevelDefinitionId { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public string Name { get; set; }
    }
}
