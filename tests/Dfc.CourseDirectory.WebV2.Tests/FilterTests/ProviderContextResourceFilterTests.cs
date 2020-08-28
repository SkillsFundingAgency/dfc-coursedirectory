﻿using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class ProviderContextResourceFilterTests : MvcTestBase
    {
        public ProviderContextResourceFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_UsesProviderFromQueryParam(TestUserType userType)
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
        public async Task AdminUser_UsesProviderFromRoute(TestUserType userType)
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
        public async Task AdminUser_QueryParamAndRouteSpecifiedButDontMatchFailsBinding(TestUserType userType)
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
        public async Task AdminUser_NoQueryParamFailsBinding(TestUserType userType)
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
        public async Task ProviderUser_UsesProviderFromAuthToken(TestUserType userType)
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
        public async Task ProviderUser_QueryParamSpecifiedDoesntMatchAuthTokenReturnsForbidden(TestUserType userType)
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
        public async Task ProviderUser_RouteParamSpecifiedDoesntMatchAuthTokenReturnsForbidden(TestUserType userType)
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
        public async Task ProviderDoesNotExistFailsBinding()
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
