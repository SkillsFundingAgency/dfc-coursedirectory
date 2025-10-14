using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateProviderNonLarsSubType : ISqlQuery<Success>
    {
        public Guid NonLarsSubTypeId { get; set; }

        public Guid ProviderId { get; set; }
    }
}
