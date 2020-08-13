using System;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class PublishedCourseViewModel
    {
        public Guid CourseId { get; set; }

        public Guid CourseRunId { get; set; }

        public string CourseName { get; set; }
    }
}