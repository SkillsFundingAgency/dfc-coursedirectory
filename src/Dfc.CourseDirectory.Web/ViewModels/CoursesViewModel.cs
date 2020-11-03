using Dfc.CourseDirectory.Services.CourseService;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class CoursesViewModel
    {
        public int? UKPRN { get; set; }
        public CourseSearchResult Courses { get; set; }
    }
}
