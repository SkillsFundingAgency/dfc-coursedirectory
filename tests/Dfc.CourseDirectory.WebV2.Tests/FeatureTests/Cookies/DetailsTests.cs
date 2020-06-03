using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Cookies
{
    public class DetailsTests : MvcTestBase
    {
        public DetailsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.Developer)]
        public async Task Get_AuthenticatedUser_ReturnsOk(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync("cookies/details");
            var doc = await response.GetDocument();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(doc.GetElementById("cookiesform"));
        }

        [Fact]
        public async Task Get_UnauthenticatedUser_ReturnsOk()
        {
            // Arrange
            User.SetNotAuthenticated();

            // Act
            var response = await HttpClient.GetAsync("cookies/details");
            var doc = await response.GetDocument();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(doc.GetElementById("cookiesform"));
        }
    }
}
