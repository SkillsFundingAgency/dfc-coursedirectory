using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Dfc.CourseDirectory.Web.Tests.IntegrationHelpers
{
    public static class VenueServiceTestFactory
    {
        public static IVenueService GetService()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<VenueService>.Instance;
            var settings = TestConfig.GetSettings<VenueServiceSettings>("VenueServiceSettings");
            IVenueService service = new VenueService(logger, new HttpClient(), Options.Create(settings));
            return service;
        }
    }
}
