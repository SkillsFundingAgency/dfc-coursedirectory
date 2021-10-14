using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Home
{
    public class IndexTests : MvcTestBase
    {
        public IndexTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task UnauthenticatedUser_RendersStartPage()
        {
            // Arrange
            User.SetNotAuthenticated();

            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementsByTagName("h1").Single().TextContent.Should().Be("Publish to the course directory");
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUser_RedirectsToHelpdeskDashboard(TestUserType userType)
        {
            // Arrange
            await User.AsTestUser(userType);

            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be("/helpdesk-dashboard");
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUser_RedirectsToProviderDashboard(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await User.AsTestUser(userType, provider.ProviderId);

            var request = new HttpRequestMessage(HttpMethod.Get, "/");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be("/dashboard");
        }

        [Fact]
        public async Task Get_Accessibility_ReturnsOk()
        {
            // Arrange

            var request = new HttpRequestMessage(HttpMethod.Get, "/accessibility");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
