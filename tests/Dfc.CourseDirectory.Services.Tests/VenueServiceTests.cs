using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Dfc.CourseDirectory.Services.VenueService;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class VenueServiceTests
    {
        [Fact]
        private async void GetByIdIsSuccess()
        {
            //var mockLogger = new Mock<ILogger<VenueService.VenueService>>();

            //var criteria = new GetVenueByIdCriteria("a63bfe28-6ac7-46c8-9efe-37dfb1bd56d7");

            //var settings = new GetVenueByIdSettings()
            //{
            //    ApiUrl = "https://dfc-dev-prov-venue-fa.azurewebsites.net/api/getvenuebyid?code=",
            //    ApiKey = ""
            //};

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

            //var settings = new GetVenueByIdSettings()
            //{
            //    ApiUrl = "https://dfc-dev-prov-venue-fa.azurewebsites.net/api/getvenuebyid?code=",
            //    ApiKey = ""
            //};

            //var service = new VenueService.VenueService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            ////// act
            //var actual = await service.GetVenueByIdAsync(criteria);

            //bool ValidReturnedVenue = actual.Value != null;

            //Assert.False(actual.IsSuccess);
            //Assert.False(ValidReturnedVenue);
        }
    }
}
