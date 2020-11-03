using System;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderCourses
{
    public class CourseDeleteConfirmViewModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
    }
}
