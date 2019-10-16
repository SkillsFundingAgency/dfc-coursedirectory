using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Tests.Unit.Helpers;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfc.CourseDirectory.Services.Tests.Unit.Mocks
{
    public static class ApprenticeshipServiceMockFactory
    {
        public static IApprenticeshipService GetApprenticeshipService(HttpClient httpClient)
        {
            var client = httpClient ?? HttpClientMockFactory.GetClient(SampleJsons.SuccessfulStandardFile(), HttpStatusCode.OK);
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ApprenticeshipService.ApprenticeshipService>.Instance;
            var settings = Options.Create(new ApprenticeshipServiceSettings{ApiKey = "this1sN0tAnapiURL", ApiUrl = "https://test.test.net/test/test/test/test"});
            return new Mock<ApprenticeshipService.ApprenticeshipService>(logger, client, settings).Object;
        }
    }
}
