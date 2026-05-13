using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseList : ISqlQuery<ListOfCourses>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
