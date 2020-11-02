using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseStatusCountResult : ICourseStatusCountResult
    {
        public int Status { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
    }
}
