
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;


namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string ValidationHeader { get; set; }
        public IEnumerable<string> ValidationMessages { get; set; }
        public int LiveCourseCount { get; set; }
        public int ArchivedCourseCount { get; set; }
        public int PendingCourseCount { get; set; }
        public IEnumerable<Course> RecentlyModifiedCourses { get; set; }
        public string SuccessHeader { get; set; }
    }
}
