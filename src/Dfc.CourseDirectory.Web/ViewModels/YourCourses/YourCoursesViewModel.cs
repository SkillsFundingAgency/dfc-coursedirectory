
using Dfc.CourseDirectory.Services.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


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
