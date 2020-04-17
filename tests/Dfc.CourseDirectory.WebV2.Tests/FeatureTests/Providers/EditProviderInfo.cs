using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers
{
    public class EditProviderInfo : MvcTestBase
    {
        public EditProviderInfo(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                alias: "Current alias",
                courseDirectoryName: "Current CD name",
                marketingInformation: "Current overview");

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal("Current alias", doc.GetElementById("Alias").As<IHtmlInputElement>().Value);
            Assert.Equal("Current CD name", doc.GetElementById("CourseDirectoryName").As<IHtmlInputElement>().Value);
            Assert.Equal("Current overview", doc.GetElementById("MarketingInformation").As<IHtmlTextAreaElement>().InnerHtml);
        }

        [Theory]
        [InlineData("", "Provider name", "Provider name")]
        [InlineData(null, "Provider name", "Provider name")]
        [InlineData("CD name", "Provider name", "CD name")]
        public async Task Get_CourseDirectoryNameIsDerivedCorrectly(
            string courseDirectoryName,
            string providerName,
            string expectedValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                providerName: providerName,
                courseDirectoryName: courseDirectoryName);

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            Assert.Equal(expectedValue, doc.GetElementById("CourseDirectoryName").As<IHtmlInputElement>().Value);
        }

        [Theory]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserNotDeveloperDoesNotRenderCourseDirectoryName(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                alias: "Current alias",
                courseDirectoryName: "Current CD name",
                marketingInformation: "Current overview");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("CourseDirectoryName"));
        }

        [Fact]
        public async Task Get_ProviderTypeIsFEDoesNotRenderBriefOverview()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("BriefOverview"));
        }

        [Fact]
        public async Task Get_UserIsDeveloperRendersEditableBriefOverview()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("MarketingInformation").GetAttribute("disabled"));
        }

        [Fact]
        public async Task Get_UserIsHelpdesk_RendersReadOnlyBriefOverview()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal("disabled", doc.GetElementById("MarketingInformation").GetAttribute("disabled"));
        }

        [Fact]
        public async Task Post_ValidRequestSubmitsSuccessfullyAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Alias", "My alias")
                .Add("CourseDirectoryName", "CD Name")
                .Add("MarketingInformation", "Overview")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={providerId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/provider/details?providerId={providerId}", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Post_AliasValidationErrorRendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Alias", new string('z', 101))
                .Add("CourseDirectoryName", "CD Name")
                .Add("MarketingInformation", "Overview")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={providerId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Alias", "Alias must be 100 characters or fewer");
        }

        [Fact]
        public async Task Post_CourseDirectoryNameValidationErrorRendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Alias", "My alias")
                .Add("CourseDirectoryName", new string('z', 101))
                .Add("MarketingInformation", "Overview")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={providerId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("CourseDirectoryName", "Course Directory name must be 100 characters or fewer");
        }

        [Fact]
        public async Task Post_MarketingInformatinoValidationErrorRendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Alias", "My alias")
                .Add("CourseDirectoryName", "CD Name")
                .Add("MarketingInformation", new string('z', 751))
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={providerId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("MarketingInformation", "Marketing information must be 750 characters or fewer");
        }
    }
}
