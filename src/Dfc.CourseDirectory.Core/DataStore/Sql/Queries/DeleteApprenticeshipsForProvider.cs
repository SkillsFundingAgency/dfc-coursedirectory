using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteApprenticeshipsForProvider : ISqlQuery<Success>
    {
        public Guid ProviderId { get; set; }
    }
}
