using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.ProviderSearch;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ProviderSearch
{
    public class ProviderSearchTests : MvcTestBase
    {
        public ProviderSearchTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderSearch_Get_WithNonAdminUser_ReturnsForbidden(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/provider-search");

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.Developer)]
        public async Task ProviderSearch_Get_WithAdminUser_ReturnsExpectedContent(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/provider-search");

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();

            doc.Body.TextContent.Should().NotContain("No providers found");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ProviderSearch_Get_WithNullOrWhiteSpaceSearchQuery_ReturnsExpectedContent(string searchQuery)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/provider-search?SearchQuery={searchQuery}");

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();

            doc.Body.TextContent.Should().Contain("No providers found");
        }

        [Fact]
        public async Task ProviderSearch_Get_WithSearchQuery_ReturnsExpectedContent()
        {
            // Arrange
            var searchQuery = "TestSearchQuery";
            var request = new HttpRequestMessage(HttpMethod.Get, $"/provider-search?SearchQuery={searchQuery}");

            var providerSearchResult1 = CreateProviderSearchResult(1);
            var providerSearchResult2 = CreateProviderSearchResult(2, ProviderStatus.Registered);
            var providerSearchResult3 = CreateProviderSearchResult(3, ProviderStatus.Unregistered);
            var providerSearchResult4 = CreateProviderSearchResult(4, providerStatus: null);

            var searchResult = new SearchResult<Provider>
            {
                Items = new[]
                {
                    new SearchResultItem<Provider> { Record = providerSearchResult1 },
                    new SearchResultItem<Provider> { Record = providerSearchResult2 },
                    new SearchResultItem<Provider> { Record = providerSearchResult3 },
                    new SearchResultItem<Provider> { Record = providerSearchResult4 }
                }
            };

            ProviderSearchClient.Setup(s => s.Search(It.Is<ProviderSearchQuery>(q => q.SearchText == searchQuery)))
                .ReturnsAsync(searchResult);

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();

            doc.Body.TextContent.Should().NotContain("No providers found");

            doc.GetElementByTestId("search-query-input").Attributes["value"].Value.Should().Be(searchQuery);

            var provider1SearchResultRow = doc.GetElementByTestId($"search-result-row-{providerSearchResult1.ProviderId}");
            var provider2SearchResultRow = doc.GetElementByTestId($"search-result-row-{providerSearchResult2.ProviderId}");
            var provider3SearchResultRow = doc.GetElementByTestId($"search-result-row-{providerSearchResult3.ProviderId}");
            var provider4SearchResultRow = doc.GetElementByTestId($"search-result-row-{providerSearchResult4.ProviderId}");

            using (new AssertionScope())
            {
                provider1SearchResultRow.Should().NotBeNull();
                provider1SearchResultRow.GetElementByTestId("provider-name").TextContent.Should().Be(providerSearchResult1.ProviderName);
                provider1SearchResultRow.GetElementByTestId("provider-action").TextContent.Trim().Should().Be("View dashboard");

                provider2SearchResultRow.Should().NotBeNull();
                provider2SearchResultRow.GetElementByTestId("provider-name").TextContent.Should().Be(providerSearchResult2.ProviderName);
                provider2SearchResultRow.GetElementByTestId("provider-action").TextContent.Trim().Should().Be("Add provider");

                provider3SearchResultRow.Should().NotBeNull();
                provider3SearchResultRow.GetElementByTestId("provider-name").TextContent.Should().Be(providerSearchResult3.ProviderName);
                provider3SearchResultRow.GetElementByTestId("provider-action").TextContent.Trim().Should().BeEmpty();

                provider4SearchResultRow.Should().NotBeNull();
                provider4SearchResultRow.GetElementByTestId("provider-name").TextContent.Should().Be(providerSearchResult4.ProviderName);
                provider4SearchResultRow.GetElementByTestId("provider-action").TextContent.Trim().Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task OnboardProvider_Post_WithNonAdminUser_ReturnsForbidden(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add(nameof(OnboardProviderCommand.ProviderId), providerId)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/provider-search/onboard")
            {
                Content = requestContent
            };

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task OnboardProvider_Post_WithAdminUser_OnboardsProviderAndRedirects(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(status: ProviderStatus.Registered);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add(nameof(OnboardProviderCommand.ProviderId), providerId)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/provider-search/onboard")
            {
                Content = requestContent
            };

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status302Found);
            response.Headers.Location.OriginalString.Should().Be($"/dashboard?providerId={providerId}");
        }

        [Fact]
        public async Task OnboardProvider_Post_WithInvalidProviderId_ReturnsNotFound()
        {
            // Arrange
            var requestContent = new FormUrlEncodedContentBuilder()
                .Add(nameof(OnboardProviderCommand.ProviderId), Guid.NewGuid())
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/provider-search/onboard")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        private static Provider CreateProviderSearchResult(
            int seed = 1,
            ProviderStatus status = ProviderStatus.Onboarded,
            string providerStatus = "Active")
        {
            return new Provider
            {
                ProviderId = Guid.NewGuid(),
                ProviderName = $"TestSearchResult{seed}",
                Postcode = $"TE1 {seed}ST",
                Town = $"TestTown{seed}",
                Ukprn = seed.ToString("00000000"),
                ProviderStatus = (int)status,
                UkrlpProviderStatusDescription = providerStatus
            };
        }
    }
}
