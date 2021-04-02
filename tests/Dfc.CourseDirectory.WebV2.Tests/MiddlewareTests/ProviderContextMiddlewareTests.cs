﻿using System;
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
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests?providerId={provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerInfo"]["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(provider.ProviderId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_RouteValueSpecified_AssignsContext(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests/from-route/{provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerInfo"]["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(provider.ProviderId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_QueryParamAndRouteSpecifiedButDontMatch_DoesNotAssignContext(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests/from-route/{provider.ProviderId}?providerId={Guid.NewGuid()}");

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
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"currentprovideractionfiltertests");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = JToken.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(Guid.TryParse(responseJson["providerInfo"]["providerId"].ToString(), out var boundProviderId), "Binding failed.");
            Assert.Equal(provider.ProviderId, boundProviderId);
        }

        [Theory]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.ProviderSuperUser)]
        public async Task ProviderUser_QueryParamSpecifiedDoesntMatchAuthToken_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

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
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

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

    [RequireProviderContext]
    public class ProviderContextModelBinderTestController : Controller
    {
        private readonly ProviderContext _providerContext;

        public ProviderContextModelBinderTestController(IProviderContextProvider providerContextProvider)
        {
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("currentprovideractionfiltertests")]
        public IActionResult Get() => Json(_providerContext);

        [HttpGet("currentprovideractionfiltertests/from-route/{providerId}")]
        public IActionResult GetFromRoute() => Json(_providerContext);
    }
}
