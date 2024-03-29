﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class RestrictProviderTypesAttributeTests : MvcTestBase
    {
        public RestrictProviderTypesAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task NoProviderContextSet_RedirectsToProviderSearch()
        {
            // Arrange
            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"RestrictProviderTypesAttributeTests");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Theory]
        [InlineData(ProviderType.None)]
        public async Task CurrentProviderDoesNotHaveAnySpecifiedPermittedProviderTypes_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: providerType);

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"RestrictProviderTypesAttributeTests?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData(ProviderType.FE)]
        public async Task CurrentProviderDoesHaveAnySpecifiedPermittedProviderTypes_ReturnsOk(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: providerType);

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"RestrictProviderTypesAttributeTests?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Route("RestrictProviderTypesAttributeTests")]
    public class RestrictProviderTypesAttributeTestsController : Controller
    {
        [RestrictProviderTypes(ProviderType.FE)]
        public IActionResult Get() => Ok();
    }
}
