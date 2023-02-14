using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers
{
    public class DisplayNameTests : MvcTestBase
    {
        public DisplayNameTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ProviderUser_ReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.Apprenticeships,
                providerName: "Provider name",
                alias: "Trading name");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"providers/display-name?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE | ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"providers/display-name?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Get_ProviderHasNoTradingName_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                alias: null);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"providers/display-name?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ProviderDisplayNameSource.ProviderName, "ProviderName")]
        [InlineData(ProviderDisplayNameSource.TradingName, "TradingName")]
        public async Task Get_ValidRequest_RendersExpectedOutput(
            ProviderDisplayNameSource displayNameSource,
            string expectedCheckedElementId)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                alias: "Trading name",
                displayNameSource: displayNameSource);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"providers/display-name?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("checked", doc.GetElementById(expectedCheckedElementId).GetAttribute("checked"));
        }

        [Fact]
        public async Task Post_ProviderUser_ReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                providerName: "Provider name",
                alias: "Trading name");

            var content = new FormUrlEncodedContentBuilder()
                .Add("DisplayNameSource", "TradingName")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"providers/display-name?providerId={provider.ProviderId}")
            {
                Content = content
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var content = new FormUrlEncodedContentBuilder()
                .Add("DisplayNameSource", "TradingName")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"providers/display-name?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Post_ProviderHasNoTradingName_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                alias: null);

            var content = new FormUrlEncodedContentBuilder()
                .Add("DisplayNameSource", "TradingName")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"providers/display-name?providerId={provider.ProviderId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesDisplayNameSourceAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider name",
                alias: "Trading name");

            var content = new FormUrlEncodedContentBuilder()
                .Add("DisplayNameSource", "TradingName")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"providers/display-name?providerId={provider.ProviderId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/providers?providerId={provider.ProviderId}", response.Headers.Location.OriginalString);

            SqlQuerySpy.VerifyQuery<SetProviderDisplayNameSource, OneOf<NotFound, Success>>(q =>
                q.ProviderId == provider.ProviderId && q.DisplayNameSource == ProviderDisplayNameSource.TradingName);
        }
    }
}
