using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers
{
    public class EditProviderTypeTests : MvcTestBase
    {
        public EditProviderTypeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotEditProviderType_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Fact]
        public async Task Get_ValidRequestNoneProviderType_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("apprenticeships").GetAttribute("checked").Should().NotBe("checked");
            doc.GetElementByTestId("fe").GetAttribute("checked").Should().NotBe("checked");
            doc.GetElementByTestId("fe+apprenticeships").GetAttribute("checked").Should().NotBe("checked");
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships, "apprenticeships")]
        [InlineData(ProviderType.FE, "fe")]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE, "fe+apprenticeships")]
        public async Task Get_ValidRequest_RendersExpectedOutput(
            ProviderType providerType,
            string expectedCheckedTestId)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: providerType);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers/provider-type?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            var option = doc.GetElementByTestId(expectedCheckedTestId);
            option.GetAttribute("checked").Should().Be("checked");
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotEditProviderType_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.FE)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)ProviderType.FE)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
        }

        [Fact]
        public async Task Post_NoProviderTypeSelected_RendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var content = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("ProviderType", "Select the provider type");
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.FE)]
        public async Task Post_ValidRequest_UpdatesProviderTypeAndRedirects(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var content = new FormUrlEncodedContentBuilder()
                .Add("ProviderType", (int)providerType)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"providers/provider-type?providerId={providerId}")
            {
                Content = content
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/providers?providerId={providerId}");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateProviderType, OneOf<NotFound, Success>>(q =>
                q.ProviderId == providerId && q.ProviderType == providerType);
        }
    }
}
