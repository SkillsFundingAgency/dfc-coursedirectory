using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CourseMigrationReport
    {
        public Guid Id { get; set; }
        public IList<Course> LarslessCourses { get; set; }
        public int PreviousLiveCourseCount { get; set; }
        public int ProviderUKPRN { get; set; }
    }
}
