using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseServiceSettings : ICourseServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int BulkUploadSecondsPerRecord { get; set; }
    }
}
