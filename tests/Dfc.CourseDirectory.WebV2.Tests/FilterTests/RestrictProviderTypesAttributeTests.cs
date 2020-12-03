using System.Net;
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
            Factory.Settings.RewriteForbiddenToNotFound = false;
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
        [InlineData(ProviderType.Apprenticeships)]
        public async Task CurrentProviderDoesNotHaveAnySpecifiedPermittedProviderTypes_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: providerType);

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"RestrictProviderTypesAttributeTests?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task CurrentProviderDoesHaveAnySpecifiedPermittedProviderTypes_ReturnsOk(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: providerType);

            await User.AsHelpdesk();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"RestrictProviderTypesAttributeTests?providerId={providerId}");

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
