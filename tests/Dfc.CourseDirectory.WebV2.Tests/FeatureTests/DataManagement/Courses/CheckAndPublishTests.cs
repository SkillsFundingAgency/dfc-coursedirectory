using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class CheckAndPublishTests : MvcTestBase
    {
        public CheckAndPublishTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_ProviderHasNoCourseUploadAtProcessedStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_CourseUploadHasErrors_RedirectsToErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, _) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should()
                .Be($"/data-upload/courses/errors?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, _) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder => rowBuilder.AddValidRows(learnAimRef, 3));

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("RowCount").TextContent.Should().Be("3");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_ProviderHasNoCourseUploadAtProcessedStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateCourseUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/check-publish?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_NotConfirmed_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (courseUpload, _) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/check-publish?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to publish these courses");
        }

        [Fact]
        public async Task Post_ValidRequest_PublishesRowsAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, _) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder => rowBuilder.AddValidRows(learnAimRef, 3));

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/check-publish?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString
                .Should().Be($"/data-upload/courses/success?providerId={provider.ProviderId}");

            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));
            courseUpload.UploadStatus.Should().Be(UploadStatus.Published);
            courseUpload.PublishedOn.Should().Be(Clock.UtcNow);

            var journeyInstance = GetJourneyInstance<PublishJourneyModel>(
                "PublishCourseUpload",
                keys => keys.With("providerId", provider.ProviderId));
            journeyInstance.State.CoursesPublished.Should().Be(3);
        }
    }
}
