using System.Net.Http;
using System.Threading.Tasks;
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
            var request = new HttpRequestMessage(HttpMethod.Get, "/tests/empty");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal(0, doc.QuerySelectorAll("#pttcd__sign-out-link").Length);
        }

        [Fact]
        public async Task AuthenticatedUser_RendersSignOutLink()
        {
            // Arrange
            // Default test setup runs with an authenticated user
            var request = new HttpRequestMessage(HttpMethod.Get, "/tests/empty");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal(1, doc.QuerySelectorAll("#pttcd__sign-out-link").Length);
        }
    }
}
