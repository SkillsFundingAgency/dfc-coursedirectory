using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLiveCourseRunCountForProvider : ISqlQuery<int>
    {
        public Guid ProviderId { get; set; }
        public bool IsNonLars { get; set; } = false;
    }
}
