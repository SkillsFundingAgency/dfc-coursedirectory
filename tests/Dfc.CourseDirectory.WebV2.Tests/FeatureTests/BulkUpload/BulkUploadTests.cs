using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.BulkUpload
{
    public class BulkUploadTests : MvcTestBase
    {
        public BulkUploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(1, "1 course")]
        [InlineData(2, "2 courses")]
        public async Task Get_PublishYourFile_RendersPendingCount(int courseCount, string expectedCourseCountText)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            for (var i = 0; i < courseCount; i++)
            {
                await TestData.CreateCourse(
                    provider.ProviderId,
                    createdBy: providerUser,
                    configureCourseRuns: builder => builder.WithCourseRun(status: CourseStatus.BulkUploadReadyToGoLive));
            }

            // Act
            var response = await HttpClient.GetAsync("/BulkUpload/PublishYourFile?NumberOfCourses=321"); // number parameter supplied by v1 code must be ignored without error for compatibility

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response.GetDocument();
            doc.GetElementByTestId("courseCount").InnerHtml.Should().Be(expectedCourseCountText);
        }
    }
}
