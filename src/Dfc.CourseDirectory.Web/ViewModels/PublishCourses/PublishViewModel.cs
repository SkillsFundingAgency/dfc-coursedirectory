using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.PublishCourses
{
    public class PublishViewModel
    {
        public IEnumerable<Course> Courses { get; set; }

        public int NumberOfCoursesInFiles { get; set; }
    }
}
