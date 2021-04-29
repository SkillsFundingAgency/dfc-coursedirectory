using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }
        public string LarsQan { get; set; }
        public string WhoIsThisCourseFor { get; set; }
        public string EntryRequirements { get; set; }
    }
}
