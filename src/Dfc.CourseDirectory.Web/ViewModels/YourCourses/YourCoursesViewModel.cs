using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.YourCourses
{
    public class YourCoursesViewModel
    {
        public string Heading { get; set; }
        public string HeadingCaption { get; set; }
        public IList<CourseViewModel> Courses { get; set;}
        public IList<QualificationLevelFilterViewModel> LevelFilters { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public int? PendingCoursesCount { get; set; }
    }
}
