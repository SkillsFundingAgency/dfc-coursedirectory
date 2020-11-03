using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.MiddlewareTests
{
    public class ProviderContextMiddlewareTests : MvcTestBase
    {
        public ProviderContextMiddlewareTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_QueryParamSpecified_AssignsContext(TestUserType userType)
        {
            // Arrange
            var ukprn = 12345;
            var providerId = await TestData.CreateProvider(ukprn);
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerInfo"]["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(providerId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_RouteValueSpecified_AssignsContext(TestUserType userType)
        {
            // Arrange
            var ukprn = 12345;
            var providerId = await TestData.CreateProvider(ukprn);
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests/from-route/{providerId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerInfo"]["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(providerId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_QueryParamAndRouteSpecifiedButDontMatch_DoesNotAssignContext(TestUserType userType)
        {
            // Arrange
            var ukprn = 12345;
            var providerId = await TestData.CreateProvider(ukprn);
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests/from-route/{providerId}?providerId={Guid.NewGuid()}");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_MissingQueryParam_DoesNotAssignContext(TestUserType userType)
        {
            // Arrange
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests?providerId=");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task ProviderUser_AssignsContextFromAuthToken(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerInfo"]["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(providerId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task ProviderUser_QueryParamSpecifiedDoesntMatchAuthToken_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests?providerId={Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task ProviderUser_RouteParamSpecifiedDoesntMatchAuthToken_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests/from-route/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AdminUser_ProviderDoesNotExist_DoesNotAssignContext()
        {
            // Arrange
            await User.AsTestUser(TestUserType.Developer);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests?providerId={Guid.NewGuid()}");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
        }
    }

    public class ProviderContextModelBinderTestController : Controller
    {
        [HttpGet("currentprovideractionfiltertests")]
        public IActionResult Get(ProviderContext providerContext) => Json(providerContext);

        [HttpGet("currentprovideractionfiltertests/from-route/{providerId}")]
        public IActionResult GetFromRoute(ProviderContext providerContext) => Json(providerContext);
    }
}
