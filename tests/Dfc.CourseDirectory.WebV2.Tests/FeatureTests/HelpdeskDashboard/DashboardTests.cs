using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.HelpdeskDashboard
{
    public class DashboardTests : TestBase
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
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
