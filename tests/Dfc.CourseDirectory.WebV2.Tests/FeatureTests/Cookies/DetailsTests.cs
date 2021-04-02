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
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync("cookies/details");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_UnauthenticatedUser_ReturnsOk()
        {
            // Arrange
            User.SetNotAuthenticated();

            // Act
            var response = await HttpClient.GetAsync("cookies/details");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
