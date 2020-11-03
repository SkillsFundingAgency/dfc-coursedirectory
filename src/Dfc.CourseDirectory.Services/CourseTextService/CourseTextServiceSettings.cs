using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextServiceSettings : ICourseTextServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
