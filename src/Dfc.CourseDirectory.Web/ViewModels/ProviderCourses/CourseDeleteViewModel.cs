using System;
using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderCourses
{
    public class CourseDeleteViewModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
        public CourseDelete CourseDelete { get; set; }
    }
}
