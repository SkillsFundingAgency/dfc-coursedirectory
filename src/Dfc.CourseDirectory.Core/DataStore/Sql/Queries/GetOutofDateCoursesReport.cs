using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetOutofDateCoursesReport : ISqlQuery<IAsyncEnumerable<OutofDateCourseItem>>
    {
    }

    public class OutofDateCourseItem
    {
        public int ProviderUkprn { get; set; }
        
        public string ProviderName { get; set; }

        public Guid CourseId { get; set; }

        public Guid CourseRunId { get; set; }

        public string CourseName { get; set; }

        public DateTime StartDate { get; set; }
    }
}
