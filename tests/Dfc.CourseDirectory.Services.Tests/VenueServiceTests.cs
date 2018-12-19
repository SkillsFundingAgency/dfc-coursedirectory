using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Controllers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class VenueServiceTests
    {
        private VenueServiceSettings settings;
        public VenueServiceTests()
        {
            settings = new VenueServiceSettings()
            {
                ApiUrl = "https://dfc-dev-prov-venue-fa.azurewebsites.net/api/",
                ApiKey = ""
            };
        }

        [Fact]
        private async void GetByIdIsSuccess()
        {
            //var mockLogger = new Mock<ILogger<VenueService.VenueService>>();

            //var criteria = new GetVenueByIdCriteria("a63bfe28-6ac7-46c8-9efe-37dfb1bd56d7");

            //var service = new VenueService.VenueService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.GetVenueByIdAsync(criteria);

            //bool ValidReturnedVenue = actual.Value != null;

            //Assert.True(actual.IsSuccess);
            //Assert.True(ValidReturnedVenue);
        }

        [Fact]
        private async void GetByIdInValidId()
        {
            //var mockLogger = new Mock<ILogger<VenueService.VenueService>>();

            //var criteria = new GetVenueByIdCriteria("fdhdhdhghghd56d7");

            //var service = new VenueService.VenueService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.GetVenueByIdAsync(criteria);

            //bool ValidReturnedVenue = actual.Value != null;

            //Assert.False(actual.IsSuccess);
            //Assert.False(ValidReturnedVenue);
        }

        [Fact]
        private async void SearchViaSingleVenueGuid()
        {
            //var mockLogger = new Mock<ILogger<VenueService.VenueService>>();

            //// arrange
            //var criteria = new VenueSearchCriteria("10000409","");

            //var service = new VenueService.VenueService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.SearchAsync(criteria);

            //Console.WriteLine(actual);
            //Assert.True(actual.IsSuccess);
        }
        [Fact]
        private async void TriggerController()
        {
            //var mockLogger = new Mock<ILogger<VenueSearchController>>();
            //var serviceLogger = new Mock<ILogger<VenueService.VenueService>>();
            //var httpContextAccessor = new Mock<IHttpContextAccessor>();
            //var session = new Mock<ISession>();

            //httpContextAccessor.Setup(x => x.HttpContext.Session).Returns(session.Object);

            //var helper = new VenueSearchHelper();

            //var service = new VenueService.VenueService(serviceLogger.Object, new HttpClient(), Options.Create(settings));

            //var controller = new VenueSearchController(
            //    logger: mockLogger.Object,
            //    venueServiceSettings: Options.Create(settings),
            //    venueService: service,
            //    venueSearchHelper: helper, 
            //    contextAccessor: httpContextAccessor.Object);


            //var reqModel = new VenueSearchRequestModel {SearchTerm = "10000409"};
            //var actual = await controller.Index(reqModel);
            //Assert.NotNull(actual);
        }

        [Fact]
        private async void AddNewVenue()
        {
            //var mockLogger = new Mock<ILogger<VenueService.VenueService>>();

            //// arrange
            //Venue venue = new Venue(
            //    null,
            //    0,
            //    "testVenueName",
            //    "testVenueAddressLine1",
            //    "testVenueAddressLine2",
            //    null,
            //    "testTown",
            //    "testCounty",
            //    "testPostCode",
            //    VenueStatus.Live,
            //    "TestUser",
            //    DateTime.Now,
            //    DateTime.Now
            //);
            //var service = new VenueService.VenueService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.AddAsync(venue);

            //Console.WriteLine(actual);
            //Assert.True(actual.IsSuccess);
        }

        [Fact]
        private async void UpdateVenue()
        {
            //var mockLogger = new Mock<ILogger<VenueService.VenueService>>();

            //// arrange
            //Venue venue = new Venue(
            //    "0f978fb4-a6ec-45ab-8178-2813fe6042cd",
            //    10007427,
            //    "testVenueName",
            //    "testVenueAddressLine1",
            //    "testVenueAddressLine2",
            //    null,
            //    "testTown",
            //    "testCounty",
            //    "testPostCode",
            //    VenueStatus.Live,
            //    "TestUser",
            //    DateTime.Now
            //);

            //var service = new VenueService.VenueService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.UpdateAsync(venue);

            //Console.WriteLine(actual);
            //Assert.True(actual.IsSuccess);
        }
    }
}
