using Dfc.CourseDirectory.Web.Controllers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net.Http;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class VenueSearchServiceTests
    {
        [Fact]
        private async void SearchViaSingleVenueGuid()
        {
            var mockLogger = new Mock<ILogger<VenueSearchService>>();
            //.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);

            // arrange
            var criteria = new VenueSearchCriteria(
                "10000409");

            var settings = new VenueSearchSettings()
            {
                ApiUrl = "http://localhost:49980/api/providers/",
                //ApiKey = "",
                ApiVersion = "2018-11-27"
            };

            var service = new VenueSearchService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            //// act
            var actual = await service.SearchAsync(criteria);

            Console.WriteLine(actual);
            Assert.True(actual.IsSuccess);
        }
        [Fact]
        private async void TriggerController()
        {
            var mockLogger = new Mock<ILogger<VenueSearchController>>();
            var serviceLogger = new Mock<ILogger<VenueSearchService>>();
            var settings = new VenueSearchSettings()
            {
                ApiUrl = "http://localhost:49980/api/providers/",
                //ApiKey = "",
                ApiVersion = "2018-11-27"
            };

            var helper = new VenueSearchHelper();

            var service = new VenueSearchService(serviceLogger.Object, new HttpClient(), Options.Create(settings));

            var controller = new VenueSearchController(
                logger: mockLogger.Object,
                venueSearchSettings: Options.Create(settings),
                venueSearchService: service,
                venueSearchHelper: helper);

           
            var reqModel = new VenueSearchRequestModel();
            reqModel.SearchTerm = "10000409";
            var actual = await controller.Index(reqModel);
            Assert.NotNull(actual);
           
        }


    }
}
