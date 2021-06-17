using System;


namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderCoursesLastUpdated : ISqlQuery<DateTime?>
    {
        public Guid ProviderId { get; set; }
    }
}
