using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class TLevelDefinition
    {
        public Guid TLevelDefinitionId { get; set; }

        public int FrameworkCode { get; set; }

        public int ProgType { get; set; }

        public int QualificationLevel { get; set; }

        public string Name { get; set; }
    }
}
