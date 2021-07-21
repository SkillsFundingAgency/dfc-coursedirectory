using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class InProgressTests : MvcTestBase
    {
        public InProgressTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_NoIncompleteUploadForProvider_ReturnsNotFound(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/in-progress?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(UploadStatus.Created, "Uploading file")]
        [InlineData(UploadStatus.Processing, "Processing data")]
        public async Task Get_UploadProcessingIsIncomplete_ReturnsLoadingView(
            UploadStatus uploadStatus,
            string expectedMessage)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/in-progress?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementsByClassName("h1").SingleOrDefault()?.TextContent.Should().Be("Uploading your course data");
            doc.GetElementById("StatusMessage").TextContent.Trim().Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData(UploadStatus.ProcessedSuccessfully, "check-publish")]
        [InlineData(UploadStatus.ProcessedWithErrors, "errors")]
        public async Task Get_UploadProcessingIsCompleted_Redirects(UploadStatus processedStatus, string expectedRedirectPath)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), processedStatus);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/in-progress?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.OriginalString.Should().Be($"/data-upload/courses/{expectedRedirectPath}?providerId={provider.ProviderId}");
        }
    }
}
