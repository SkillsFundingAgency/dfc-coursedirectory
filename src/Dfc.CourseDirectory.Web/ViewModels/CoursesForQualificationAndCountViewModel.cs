using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class CoursesForQualificationAndCountViewModel
    {
        public Course Course { get; set; }

        public IEnumerable<Course> Courses { get; set; }

        public int CourseRunCount { get; set; }

        public IEnumerable<CourseRun> CourseRuns { get; set; }

        public string QualificationType { get; set; }
    }
}