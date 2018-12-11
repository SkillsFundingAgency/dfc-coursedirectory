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
    public class VenueAddServiceTests
    {
        [Fact]
        private async void AddNewVenue()
        {
            //var mockLogger = new Mock<ILogger<VenueAddService>>();

            //// arrange
            //var venue = new VenueAdd("testaddress1","testaddress2","testtown","testvenuename","testcounty","testpostcode");

            //var settings = new VenueAddSettings()
            //{
            //    ApiUrl = "https://dfc-dev-prov-venue-fa.azurewebsites.net/api",
            //    ApiKey = ""
            //};

            //var service = new VenueAddService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.AddAsync(venue);

            //Console.WriteLine(actual);
            //Assert.True(actual.IsSuccess);
        }
       


    }
}
