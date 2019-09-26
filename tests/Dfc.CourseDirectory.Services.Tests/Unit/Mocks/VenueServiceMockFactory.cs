using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.Tests.Unit.Helpers;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfc.CourseDirectory.Services.Tests.Unit.Mocks
{
    public static class VenueServiceMockFactory
    {
        public static IVenueService GetVenueService(HttpClient httpClient)
        {
            var client = httpClient ?? HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<VenueService.VenueService>.Instance;
            var settings = Options.Create(new VenueServiceSettings { ApiKey = "this1sN0tAnapiURL", ApiUrl = "https://test.test.net/test/test/test/test" });
            return new Mock<VenueService.VenueService>(logger, client, settings).Object;
        }
    }


}
