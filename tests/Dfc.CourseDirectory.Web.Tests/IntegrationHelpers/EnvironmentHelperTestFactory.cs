using Dfc.CourseDirectory.Models.Models.Environment;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Tests.IntegrationHelpers
{
    public static class EnvironmentHelperTestFactory
    {
        public static IEnvironmentHelper GetService()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<EnvironmentHelper>.Instance;
            var settings = TestConfig.GetSettings<EnvironmentSettings>("ProviderServiceSettings");
            IEnvironmentHelper service = new EnvironmentHelper(Options.Create<EnvironmentSettings>(new EnvironmentSettings() { EnvironmentName = "Test" }));
            return service;
        }
    }
}
