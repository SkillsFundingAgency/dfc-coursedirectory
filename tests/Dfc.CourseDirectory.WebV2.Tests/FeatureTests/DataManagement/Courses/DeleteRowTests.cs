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
    public class DeleteRowTests : MvcTestBase
    {
        public DeleteRowTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(54)]
        public async Task Get_DeleteNonExistentRowNumber_ReturnsError(int rowNumber)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_DeleteExistingCoursesRow_ReturnOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.IsValid = false;
                        record.CourseName = string.Empty;
                    });
                });

            var courseRunId = courseUploadRows.First().RowNumber;

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/resolve/{courseRunId}/details/delete?providerId={provider.ProviderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_DeletedCoursesRow_ReturnError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.IsValid = false;
                        record.CourseName = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var deleteRequest = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };
            var deleteResponse = await HttpClient.SendAsync(deleteRequest);
            deleteResponse.EnsureNonErrorStatusCode();

            // Act
            var getResponse = await HttpClient.GetAsync($"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public async Task Post_DeleteInvalid_ReturnsError(int rowNumber)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.IsValid = false;
                        record.CourseName = string.Empty;
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_DeleteValidUnconfirmed_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.IsValid = false;
                        record.CourseName = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "false")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_DeleteValid_ReturnsOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.IsValid = false;
                        record.CourseName = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().Be($"/data-upload/courses/check-publish?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Post_DeleteDeletedCourse_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.Errors = new[] { "COURSERUN_COURSE_NAME_REQUIRED" };
                        record.IsValid = false;
                        record.CourseName = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };
            var response = await HttpClient.SendAsync(request);
            response.EnsureNonErrorStatusCode();

            // Act
            var request2 = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/details/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            var response2 = await HttpClient.SendAsync(request2);

            // Assert
            response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
