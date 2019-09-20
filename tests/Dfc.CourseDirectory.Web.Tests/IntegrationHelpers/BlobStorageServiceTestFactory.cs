using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Tests.IntegrationHelpers
{
    public static class BlobStorageServiceTestFactory
    {
        public static IBlobStorageService GetService()
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<BlobStorageService>.Instance;
            var settings = TestConfig.GetSettings<BlobStorageSettings>("BlobStorageSettings");
            IBlobStorageService service = new BlobStorageService(logger, new System.Net.Http.HttpClient(), Options.Create(settings));
            return service;
        }
    }
}
