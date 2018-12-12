using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class ProviderSearchServiceTests
    {
        [Fact]
        private async void SearchForProviderIsSuccess()
        {
            var mockLogger = new Mock<ILogger<ProviderSearchService>>();

            var criteria = new ProviderSearchCriteria("10028217");

            var settings = new ProviderSearchSettings()
            {
                ApiUrl = "https://dfc-dev-prov-ukrlp-fa.azurewebsites.net/api/GetProviderByPRN?code=",
                ApiKey = ""
            };

            var service = new ProviderSearchService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            //// act
            var actual = await service.SearchAsync(criteria);

            bool ValidReturnedProvider = false;
            foreach(var item in actual.Value.Value)
            {
                if (item != null)
                    ValidReturnedProvider = true;
            }

            Assert.True(actual.IsSuccess);
            Assert.True(ValidReturnedProvider);
        }

        [Fact]
        private async void SearchForProviderIsSuccessNoResult()
        {
            var mockLogger = new Mock<ILogger<ProviderSearchService>>();


            var criteria = new ProviderSearchCriteria("AbraKadabra");

            var settings = new ProviderSearchSettings()
            {
                ApiUrl = "https://dfc-dev-prov-ukrlp-fa.azurewebsites.net/api/GetProviderByPRN?code=",
                ApiKey = ""
            };

            var service = new ProviderSearchService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            //// act
            var actual = await service.SearchAsync(criteria);

            bool ValidReturnedProvider = false;
            foreach (var item in actual.Value.Value)
            {
                if (item != null)
                    ValidReturnedProvider = true;
            }

            Assert.True(actual.IsSuccess);
            Assert.False(ValidReturnedProvider); // Does NOT find provider => Assert.False
        }

        [Fact]
        private async void SearchForProviderIsFailure()
        {
            var mockLogger = new Mock<ILogger<ProviderSearchService>>();

            var criteria = new ProviderSearchCriteria("10028217");

            var settings = new ProviderSearchSettings()
            {
                ApiUrl = "IsFailurehttps://dfc-dev-prov-ukrlp-fa.azurewebsites.net/api/GetProviderByPRN?code=",
                ApiKey = ""
            };

            var service = new ProviderSearchService(mockLogger.Object, new HttpClient(), Options.Create(settings));

            //// act
            var actual = await service.SearchAsync(criteria);

            Assert.True(actual.IsFailure);
        }
    }
}
