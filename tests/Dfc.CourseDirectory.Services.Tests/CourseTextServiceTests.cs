using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Xunit;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.ProviderService;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class CourseTextServiceTests
    {
       
        private CourseTextServiceSettings settings;
        public CourseTextServiceTests()
        {
            settings = new CourseTextServiceSettings()
            {
                //ApiUrl = "https://dfc-dev-prov-venue-fa.azurewebsites.net/api/",
                ApiUrl = "http://localhost:7071/api/", //DEBUG URL
                ApiKey = ""
            };
        }

        [Fact]
        private async void GetExemplarTextForValidLARSIsSuccess()
        {
            //var mockLogger = new Mock<ILogger<CourseTextService.CourseTextService>>();

            //var criteria = new CourseTextServiceCriteria("5004767X");

            //var service = new CourseTextService.CourseTextService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.GetCourseTextByLARS(criteria);

            //Assert.True(actual.IsSuccess);
            //Assert.True(actual.Value.LearnAimRef== "5004767X");
        }

        [Fact]
        private async void SearchForProviderIsSuccessNoResult()
        {
            //var mockLogger = new Mock<ILogger<CourseTextService.CourseTextService>>();

            //var criteria = new CourseTextServiceCriteria("blah");

            //var service = new CourseTextService.CourseTextService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.GetCourseTextByLARS(criteria);

            //Assert.True(actual.IsFailure);
        }

    }
}
