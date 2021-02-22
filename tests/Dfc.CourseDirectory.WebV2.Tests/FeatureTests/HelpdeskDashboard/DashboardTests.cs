using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.HelpdeskDashboard
{
    public class DashboardTests : MvcTestBase
    {
        public DashboardTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUserCannotAccess(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task Get_AdminsCanAccess(TestUserType userType)
        {
            // Arrange
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task Get_RendersDownloadProviderTypeReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadProviderTypeReportLink = doc.GetElementByTestId("download-provider-type-report-link");

            downloadProviderTypeReportLink.Should().NotBeNull();
            downloadProviderTypeReportLink.TextContent.Should().Be("Download provider type report");
            downloadProviderTypeReportLink.Attributes["href"].Value.Should().Be("/providers/reports/provider-type");
        }

        [Fact]
        public async Task Get_RendersDownloadLiveTLevelsReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadLiveTLevelsReportLink = doc.GetElementByTestId("download-live-tlevels-report-link");

            downloadLiveTLevelsReportLink.Should().NotBeNull();
            downloadLiveTLevelsReportLink.TextContent.Should().Be("Download live T Levels report");
            downloadLiveTLevelsReportLink.Attributes["href"].Value.Should().Be("/t-levels/reports/live-t-levels");
        }
    }
}
