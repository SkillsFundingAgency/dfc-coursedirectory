using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Tests.IntegrationHelpers
{
    public static class ApprenticeshipServiceTestFactory
    {
        public static IApprenticeshipService GetService()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipService>.Instance;
            var settings = TestConfig.GetSettings<ApprenticeshipServiceSettings>("ApprenticeshipServiceSettings");
            IApprenticeshipService service = new ApprenticeshipService(logger, new System.Net.Http.HttpClient(), Options.Create(settings));
            return service;
        }
    }
}
