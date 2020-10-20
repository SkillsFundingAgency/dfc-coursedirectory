using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class LayoutTests : MvcTestBase
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
            doc.QuerySelectorAll(".pttcd-sign-out-navigation-item").Length.Should().Be(0);
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
            doc.QuerySelectorAll(".pttcd-sign-out-navigation-item").Length.Should().Be(1);
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

            topLevelLinks.Count.Should().Be(6);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-qa");
                topLevelLinks[2].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[3].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[4].TestId.Should().Be("topnav-migrationreports");
                topLevelLinks[5].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
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

            topLevelLinks.Count.Should().Be(6);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-qa");
                topLevelLinks[2].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[3].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[4].TestId.Should().Be("topnav-migrationreports");
                topLevelLinks[5].TestId.Should().Be("topnav-signout");
            }

            Assert.Equal(4, subNavLinks.Count);

            using (new AssertionScope())
            {
                subNavLinks[0].TestId.Should().Be("adminsubnav-home");
                subNavLinks[1].TestId.Should().Be("adminsubnav-courses");
                subNavLinks[2].TestId.Should().Be("adminsubnav-locations");
                subNavLinks[3].TestId.Should().Be("adminsubnav-bulkupload");
                subNavLinks[3].Href.Should().Be("/BulkUpload");
            }
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

            topLevelLinks.Count.Should().Be(6);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-qa");
                topLevelLinks[2].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[3].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[4].TestId.Should().Be("topnav-migrationreports");
                topLevelLinks[5].TestId.Should().Be("topnav-signout");
            }

            Assert.Equal(4, subNavLinks.Count);

            using (new AssertionScope())
            {
                subNavLinks[0].TestId.Should().Be("adminsubnav-home");
                subNavLinks[1].TestId.Should().Be("adminsubnav-apprenticeships");
                subNavLinks[2].TestId.Should().Be("adminsubnav-locations");
                subNavLinks[3].TestId.Should().Be("adminsubnav-bulkupload");
                subNavLinks[3].Href.Should().Be("/BulkUploadApprenticeships");
            }
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

            topLevelLinks.Count.Should().Be(6);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-qa");
                topLevelLinks[2].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[3].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[4].TestId.Should().Be("topnav-migrationreports");
                topLevelLinks[5].TestId.Should().Be("topnav-signout");
            }

            Assert.Equal(5, subNavLinks.Count);

            using (new AssertionScope())
            {
                subNavLinks[0].TestId.Should().Be("adminsubnav-home");
                subNavLinks[1].TestId.Should().Be("adminsubnav-courses");
                subNavLinks[2].TestId.Should().Be("adminsubnav-apprenticeships");
                subNavLinks[3].TestId.Should().Be("adminsubnav-locations");
                subNavLinks[4].TestId.Should().Be("adminsubnav-bulkupload");
                subNavLinks[4].Href.Should().Be("/BulkUpload/LandingOptions");
            }
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

            topLevelLinks.Count.Should().Be(5);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-courses");
                topLevelLinks[2].TestId.Should().Be("topnav-locations");
                topLevelLinks[3].TestId.Should().Be("topnav-bulkupload");
                topLevelLinks[3].Href.Should().Be("/BulkUpload");
                topLevelLinks[4].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
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

            topLevelLinks.Count.Should().Be(5);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-apprenticeships");
                topLevelLinks[2].TestId.Should().Be("topnav-locations");
                topLevelLinks[3].TestId.Should().Be("topnav-bulkupload");
                topLevelLinks[3].Href.Should().Be("/BulkUploadApprenticeships");
                topLevelLinks[4].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
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

            topLevelLinks.Count.Should().Be(6);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-courses");
                topLevelLinks[2].TestId.Should().Be("topnav-apprenticeships");
                topLevelLinks[3].TestId.Should().Be("topnav-locations");
                topLevelLinks[4].TestId.Should().Be("topnav-bulkupload");
                topLevelLinks[4].Href.Should().Be("/BulkUpload/LandingOptions");
                topLevelLinks[5].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
        }

        [Theory]
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
            bulkUploadLink.GetAttribute("href").Should().Be(expectedHref);
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
            bulkUploadLink.GetAttribute("href").Should().Be(expectedHref);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserProviderNotPassedQA_DoesNotRenderApprenticeshipsLink(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Both,
                providerName: "Test Provider",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("topnav-apprenticeships").Should().BeNull();
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserProviderNotPassedQA_DoesNotRenderApprenticeshipsLink(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Both,
                providerName: "Test Provider",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("adminsubnav-apprenticeships").Should().BeNull();
        }

        [Fact]
        public async Task NoCookiePreferencesSet_RendersCookieBanner()
        {
            // Arrange
            CookieSettingsProvider.SetPreferencesForCurrentUser(null);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            var doc = await response.GetDocument();
            doc.GetAllElementsByTestId("cookie-banner").Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CookiePreferencesSet_DoesNotRenderCookieBanner(bool allowAnalyticsCookies)
        {
            // Arrange
            CookieSettingsProvider.SetPreferencesForCurrentUser(new Cookies.CookieSettings()
            {
                AllowAnalyticsCookies = allowAnalyticsCookies
            });

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            var doc = await response.GetDocument();
            doc.GetElementByTestId("cookie-banner").Should().BeNull();
        }

        [Fact]
        public async Task AllCookiesAccepted_RendersConfirmation()
        {
            // Arrange
            CookieSettingsProvider.SetPreferencesForCurrentUser(null);

            // Act
            var response = await HttpClient.PostAsync("cookies/accept-all?returnUrl=/foo", null);

            // Assert
            var doc = await response.GetDocument();
            doc.GetAllElementsByTestId("cookie-banner-confirmation").Should().NotBeNull();
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public async Task RendersGATrackingCodeBasedOnUsersPreferences(
            bool? allowAnalyticsCookies,
            bool expectGATagsToBeRendered)
        {
            // Arrange
            Cookies.CookieSettings settings = null;
            if (allowAnalyticsCookies != null)
            {
                settings = new Cookies.CookieSettings() { AllowAnalyticsCookies = allowAnalyticsCookies.Value };
            }

            CookieSettingsProvider.SetPreferencesForCurrentUser(settings);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            var doc = await response.GetDocument();

            var gotGATags = doc.QuerySelectorAll("script")
                .Where(s => s.GetAttribute("src")?.StartsWith("https://www.googletagmanager.com") == true)
                .Any();
            gotGATags.Should().Be(expectGATagsToBeRendered);
        }

        private IReadOnlyList<(string Href, string TestId)> GetTopLevelNavLinks(IHtmlDocument doc)
        {
            var results = new List<(string Href, string TestId)>();

            foreach (var item in doc.GetElementsByClassName("govuk-header__navigation-item"))
            {
                var anchor = item.GetElementsByTagName("a")[0];
                var href = anchor.GetAttribute("href");
                var testId = anchor.GetAttribute("data-testid");

                results.Add((href, testId));
            }

            return results;
        }

        private IReadOnlyList<(string Href, string TestId)> GetSubNavLinks(IHtmlDocument doc)
        {
            var results = new List<(string Href, string TestId)>();

            foreach (var item in doc.GetElementsByClassName("pttcd-subnav__item"))
            {
                var anchor = item.GetElementsByTagName("a")[0];
                var href = anchor.GetAttribute("href");
                var testId = anchor.GetAttribute("data-testid");

                results.Add((href, testId));
            }

            return results;
        }
    }
}
