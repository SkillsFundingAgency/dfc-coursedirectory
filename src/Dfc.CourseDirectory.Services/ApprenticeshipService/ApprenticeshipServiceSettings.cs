using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class ApprenticeshipServiceSettings : IApprenticeshipServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
