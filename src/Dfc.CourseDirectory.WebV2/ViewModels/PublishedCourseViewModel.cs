using System;

namespace Dfc.CourseDirectory.WebV2.ViewModels
{
    public class PublishedCourseViewModel
    {
        public Guid CourseId { get; set; }

        public Guid CourseRunId { get; set; }

        public string CourseName { get; set; }

        public bool NonLarsCourse { get; set; }
    }
}
