using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class NonLarsSubType
    {
        public Guid NonLarsSubTypeId { get; set; }

        public string Name { get; set; }

        public DateTime AddedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public int IsActive { get; set; }
    }
}
