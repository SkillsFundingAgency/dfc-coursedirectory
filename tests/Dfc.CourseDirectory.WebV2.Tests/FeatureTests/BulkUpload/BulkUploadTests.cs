using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.BulkUpload
{
    public class BulkUploadTests :MvcTestBase
    {
        public BulkUploadTests(CourseDirectoryApplicationFactory factory) : base(factory) { }

        [Theory]
        [InlineData(1,"1 course")]
        [InlineData(2,"2 courses")]
        public async Task Get_PublishYourFile_RendersPendingCount(int courseCount, string expectedCourseCountText)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, providerId);
            var providerUser = await TestData.CreateUser(providerId: providerId);
            for (var i = 0; i < courseCount; i++)
            {
                await TestData.CreateCourse(providerId, createdBy: providerUser, courseStatus: CourseStatus.BulkUploadReadyToGoLive);
            }

            // Act
            var response = await HttpClient.GetAsync("/BulkUpload/PublishYourFile?NumberOfCourses=321"); // number parameter supplied by v1 code must be ignored without error for compatibility

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response.GetDocument();
            doc.GetElementByTestId("courseCount").InnerHtml.Should().Be(expectedCourseCountText);
        }

        [Theory]
        [InlineData(1000, "1000 courses", "17 minutes")]
        public async Task Get_PublishingYourFile_RendersCountAndTime(int courseCount, string expectedCourseCountText, string expectedTimeEstimate)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, providerId);

            // Act
            var response = await HttpClient.GetAsync("/bulk-upload/publishing-your-file?NumberOfCourses=" + courseCount);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("courseCount").InnerHtml.Should().Be(expectedCourseCountText);
                doc.GetElementByTestId("timeEstimate").InnerHtml.Should().Be(expectedTimeEstimate);
            }
        }
    }
}
