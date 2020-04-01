using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class LayoutTests : TestBase
    {
        public LayoutTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task UnauthenticatedUser_DoesNotRenderSignOutLink()
        {
            // Arrange
            User.SetNotAuthenticated();
            var request = new HttpRequestMessage(HttpMethod.Get, "/tests/empty-provider-context");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal(0, doc.QuerySelectorAll(".pttcd-sign-out-navigation-item").Length);
        }

        [Fact]
        public async Task AuthenticatedUser_RendersSignOutLink()
        {
            // Arrange
            // Default test setup runs with an authenticated user
            var request = new HttpRequestMessage(HttpMethod.Get, "/tests/empty-provider-context");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal(1, doc.QuerySelectorAll(".pttcd-sign-out-navigation-item").Length);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithoutProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync("/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(5, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Quality assurance", topLevelLinks[1].label);
            Assert.Equal("Search providers", topLevelLinks[2].label);
            Assert.Equal("Migration reports", topLevelLinks[3].label);
            Assert.Equal("Sign out", topLevelLinks[4].label);

            Assert.Equal(0, subNavLinks.Count);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithFEOnlyProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(5, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Quality assurance", topLevelLinks[1].label);
            Assert.Equal("Search providers", topLevelLinks[2].label);
            Assert.Equal("Migration reports", topLevelLinks[3].label);
            Assert.Equal("Sign out", topLevelLinks[4].label);

            Assert.Equal(4, subNavLinks.Count);
            Assert.Equal("Home", subNavLinks[0].label);
            Assert.Equal("Your courses", subNavLinks[1].label);
            Assert.Equal("Your locations", subNavLinks[2].label);
            Assert.Equal("Bulk upload", subNavLinks[3].label);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithApprenticeshipsOnlyProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(5, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Quality assurance", topLevelLinks[1].label);
            Assert.Equal("Search providers", topLevelLinks[2].label);
            Assert.Equal("Migration reports", topLevelLinks[3].label);
            Assert.Equal("Sign out", topLevelLinks[4].label);

            Assert.Equal(4, subNavLinks.Count);
            Assert.Equal("Home", subNavLinks[0].label);
            Assert.Equal("Your apprenticeships training", subNavLinks[1].label);
            Assert.Equal("Your locations", subNavLinks[2].label);
            Assert.Equal("Bulk upload", subNavLinks[3].label);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithBothFEAndApprenticeshipsProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Both,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(5, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Quality assurance", topLevelLinks[1].label);
            Assert.Equal("Search providers", topLevelLinks[2].label);
            Assert.Equal("Migration reports", topLevelLinks[3].label);
            Assert.Equal("Sign out", topLevelLinks[4].label);

            Assert.Equal(5, subNavLinks.Count);
            Assert.Equal("Home", subNavLinks[0].label);
            Assert.Equal("Your courses", subNavLinks[1].label);
            Assert.Equal("Your apprenticeships training", subNavLinks[2].label);
            Assert.Equal("Your locations", subNavLinks[3].label);
            Assert.Equal("Bulk upload", subNavLinks[4].label);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserForFEOnlyProvider_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(5, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Your courses", topLevelLinks[1].label);
            Assert.Equal("Your locations", topLevelLinks[2].label);
            Assert.Equal("Bulk upload", topLevelLinks[3].label);
            Assert.Equal("Sign out", topLevelLinks[4].label);

            Assert.Equal(0, subNavLinks.Count);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserForApprenticeshipsOnlyProvider_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(5, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Your apprenticeships training", topLevelLinks[1].label);
            Assert.Equal("Your locations", topLevelLinks[2].label);
            Assert.Equal("Bulk upload", topLevelLinks[3].label);
            Assert.Equal("Sign out", topLevelLinks[4].label);

            Assert.Equal(0, subNavLinks.Count);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserForBothFEAndApprenticeships_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Both,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            Assert.Equal(6, topLevelLinks.Count);
            Assert.Equal("Home", topLevelLinks[0].label);
            Assert.Equal("Your courses", topLevelLinks[1].label);
            Assert.Equal("Your apprenticeships training", topLevelLinks[2].label);
            Assert.Equal("Your locations", topLevelLinks[3].label);
            Assert.Equal("Bulk upload", topLevelLinks[4].label);
            Assert.Equal("Sign out", topLevelLinks[5].label);

            Assert.Equal(0, subNavLinks.Count);
        }

        [Theory]
        [InlineData(ProviderType.FE, "/BulkUpload")]
        [InlineData(ProviderType.Apprenticeships, "/BulkUploadApprenticeships")]
        [InlineData(ProviderType.Both, "/BulkUpload/LandingOptions")]
        public async Task AdminProviderContextNavBulkUploadLinksAreCorrect(
            ProviderType providerType,
            string expectedHref)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: providerType,
                providerName: "Test Provider");

            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var bulkUploadLink = doc.GetElementsByTagName("a").Single(a => a.TextContent.Trim() == "Bulk upload");
            Assert.Equal(expectedHref, bulkUploadLink.GetAttribute("href"));
        }

        [Theory]
        [InlineData(ProviderType.FE, "/BulkUpload")]
        [InlineData(ProviderType.Apprenticeships, "/BulkUploadApprenticeships")]
        [InlineData(ProviderType.Both, "/BulkUpload/LandingOptions")]
        public async Task ProviderTopNavBulkUploadLinksAreCorrect(
            ProviderType providerType,
            string expectedHref)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: providerType,
                providerName: "Test Provider");

            await User.AsProviderUser(providerId, providerType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");
            
            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var bulkUploadLink = doc.GetElementsByTagName("a").Single(a => a.TextContent.Trim() == "Bulk upload");
            Assert.Equal(expectedHref, bulkUploadLink.GetAttribute("href"));
        }

        private IReadOnlyList<(string href, string label)> GetTopLevelNavLinks(IHtmlDocument doc)
        {
            var results = new List<(string href, string label)>();

            foreach (var item in doc.GetElementsByClassName("govuk-header__navigation-item"))
            {
                var anchor = item.GetElementsByTagName("a")[0];
                var href = anchor.GetAttribute("href");
                var label = anchor.TextContent.Trim();

                results.Add((href, label));
            }

            return results;
        }

        private IReadOnlyList<(string href, string label)> GetSubNavLinks(IHtmlDocument doc)
        {
            var results = new List<(string href, string label)>();

            foreach (var item in doc.GetElementsByClassName("pttcd-subnav__item"))
            {
                var anchor = item.GetElementsByTagName("a")[0];
                var href = anchor.GetAttribute("href");
                var label = anchor.TextContent.Trim();

                results.Add((href, label));
            }

            return results;
        }
    }
}
