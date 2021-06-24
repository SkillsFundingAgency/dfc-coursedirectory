using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class ResolveRowDetailsTests : MvcTestBase
    {
        public ResolveRowDetailsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var rowNumber = courseUploadRows.Max(r => r.RowNumber) + 1;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_RowDoesNotHaveErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully);

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var rowNumber = courseUploadRows.Max(r => r.RowNumber) + 1;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", "Online")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_RowDoesNotHaveErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully);

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", "Online")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_DeliveryModeNotProvided_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("DeliveryMode", "Select how the course will be delivered");
        }

        [Theory]
        [InlineData(CourseDeliveryMode.ClassroomBased, "classroom")]
        [InlineData(CourseDeliveryMode.Online, "online")]
        [InlineData(CourseDeliveryMode.WorkBased, "work")]
        public async Task Post_ValidRequest_ReturnsRedirect(
            CourseDeliveryMode deliveryMode,
            string expectedRedirectLocationDeliveryQueryParam)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/delivery?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("DeliveryMode", deliveryMode)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should().Be(
                $"/data-upload/courses/resolve/{rowNumber}/details?deliveryMode={expectedRedirectLocationDeliveryQueryParam}&providerId={provider.ProviderId}");
        }
    }
}
