using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
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
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedWithErrors);

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/resolve/{rowNumber}/details/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_DeleteExistingCoursesRow_ReturnOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                  configureRows: rowBuilder =>
                  {
                      rowBuilder.AddValidRow("some Radndom Ref");
                  });
            var courseRunId = courseUploadRows.FirstOrDefault().RowNumber;

            // Act
            var response = await HttpClient.GetAsync($"/data-upload/courses/resolve/{courseRunId}/details/delete");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact]
        public async Task Get_DeletedCoursesRow_ReturnError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                  configureRows: rowBuilder =>
                  {
                      rowBuilder.AddValidRow("some Radndom Ref");
                  });
            var rowNumber = courseUploadRows.FirstOrDefault().RowNumber;

            // Act
            var postrequest = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/Courses/resolve/{rowNumber}/details/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var postresponse = await HttpClient.SendAsync(postrequest);
            var getresponse = await HttpClient.GetAsync($"/data-upload/courses/resolve/{rowNumber}/details/delete");

            // Assert
            postresponse.StatusCode.Should().Be(HttpStatusCode.Found);
            getresponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public async Task Post_DeleteInvalid_ReturnsError(int rowNumber)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                  configureRows: rowBuilder =>
                  {
                      rowBuilder.AddValidRow("some Radndom Ref");
                  });

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/Courses/resolve/{rowNumber}/details/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_DeleteValidUnconfirmed_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                  configureRows: rowBuilder =>
                  {
                      rowBuilder.AddValidRow("some Radndom Ref");
                  });
            var rowNumber = courseUploadRows.FirstOrDefault().RowNumber;

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/Courses/resolve/{rowNumber}/details/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "false")
                .ToContent()
            };
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_DeleteValid_ReturnsOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                  configureRows: rowBuilder =>
                  {
                      rowBuilder.AddValidRow("some Radndom Ref");
                  });
            var rowNumber = courseUploadRows.FirstOrDefault().RowNumber;

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/Courses/resolve/{rowNumber}/details/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
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
            await User.AsTestUser(TestUserType.ProviderUser, provider.ProviderId);
            Clock.UtcNow = new DateTime(2021, 4, 9, 13, 0, 0);
            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                  configureRows: rowBuilder =>
                  {
                      rowBuilder.AddValidRow("some Radndom Ref");
                  });
            var rowNumber = courseUploadRows.FirstOrDefault().RowNumber;

            // Act
            var request = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/Courses/resolve/{rowNumber}/details/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };
            var request2 = new HttpRequestMessage(
            HttpMethod.Post, $"/data-upload/Courses/resolve/{rowNumber}/details/delete")
            {
                Content = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent()
            };

            var response = await HttpClient.SendAsync(request);
            var response2 = await HttpClient.SendAsync(request2);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().Be($"/data-upload/courses/check-publish?providerId={provider.ProviderId}");
            response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
