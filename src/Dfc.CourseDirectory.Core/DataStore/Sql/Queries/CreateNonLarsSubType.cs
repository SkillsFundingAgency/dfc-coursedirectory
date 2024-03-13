using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateNonLarsSubType : ISqlQuery<Success>
    {
        public Guid NonLarsSubTypeId { get; set; }

        public string Name { get; set; }

        public DateTime AddedOn { get; set; }

        public int IsActive { get; set; }
    }
}
