using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Cookies
{
    public class AcceptCookiesTests : MvcTestBase
    {
        public AcceptCookiesTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Post_ReturnsRedirect()
        {
            // Arrange

            // Act
            var response = await HttpClient.PostAsync("cookies/accept-all?returnUrl=/foo", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/foo", response.Headers.Location.OriginalString);
        }
    }
}
