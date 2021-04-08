using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement
{
    public class VenueUploadTests : MvcTestBase
    {
        public VenueUploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_DataManagement_RendersPage()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync("/data-upload/venues"); 

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
