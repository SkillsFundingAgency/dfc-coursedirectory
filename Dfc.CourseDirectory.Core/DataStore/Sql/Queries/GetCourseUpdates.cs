using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseUpdates : ISqlQuery<ListOfCourseUpdates>
    {
        public DateTime CutOffDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
