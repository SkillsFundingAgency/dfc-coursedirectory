using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels.ProviderCourses;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class ProviderCoursesViewModel
    {


        public List<ProviderCourseRunViewModel> CoursesRuns { get; set; }
    }
}