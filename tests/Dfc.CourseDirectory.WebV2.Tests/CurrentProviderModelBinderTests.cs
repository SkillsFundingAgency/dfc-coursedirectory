﻿using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
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
            var ukprn = 12345;
            var providerId = await TestData.CreateProvider(ukprn);
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
            var ukprn = 12345;
            var specifiedUkprn = 67890;
            var providerId = await TestData.CreateProvider(ukprn);
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
        [AllowNoCurrentProvider]  // Prevent filter from modifying response
        public IActionResult Get(ProviderInfo providerInfo) => Json(providerInfo);
    }
}
