using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.PublishCourses
{
    public class PublishViewModel
    {
        public IEnumerable<Course> Courses { get; set; }

        public int NumberOfCoursesInFiles { get; set; }

        public PublishMode PublishMode { get; set; }

        public bool AreAllReadyToBePublished { get; set; }

        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
    }
}
