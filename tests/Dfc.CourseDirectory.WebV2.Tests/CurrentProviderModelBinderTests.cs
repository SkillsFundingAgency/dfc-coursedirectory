using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class CurrentProviderModelBinderTests : TestBase
    {
        public CurrentProviderModelBinderTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_UsesProviderFromQueryParam(TestUserType userType)
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var ukprn = 12345;

            Factory.ProviderInfoCache
                .Setup(mock => mock.GetProviderInfo(ukprn))
                .ReturnsAsync(new ProviderInfo()
                {
                    ProviderId = providerId,
                    UKPRN = ukprn
                });

            User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovidermodelbindertests?ukprn={ukprn}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(providerId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_NoQueryParamFailsBinding(TestUserType userType)
        {
            // Arrange
            User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovidermodelbindertests?ukprn=");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(JTokenType.Null, responseJson.Type);
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task ProviderUser_UsesProviderFromAuthToken(TestUserType userType)
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var ukprn = 12345;

            var specifiedUkprn = 67890;

            Factory.ProviderInfoCache
                .Setup(mock => mock.GetProviderInfo(ukprn))
                .ReturnsAsync(new ProviderInfo()
                {
                    ProviderId = providerId,
                    UKPRN = ukprn
                });

            User.AsTestUser(userType, ukprn);

            // Act
            var response = await HttpClient.GetAsync($"currentprovidermodelbindertests?ukprn={specifiedUkprn}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(providerId, boundProviderId);
        }
    }

    public class CurrentProviderModelBinderTestController : Controller
    {
        [HttpGet("currentprovidermodelbindertests")]
        public IActionResult Get(ProviderInfo providerInfo) => Json(providerInfo);
    }
}
