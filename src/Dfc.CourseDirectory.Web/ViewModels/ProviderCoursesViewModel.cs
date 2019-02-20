using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class ProviderCoursesViewModel
    {
        public string QualificationTitle {get; set; }
        public string QualificationType { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? CourseRunId { get; set; }

        public IEnumerable<Course> CoursesForLevel { get; set; }
    }
}