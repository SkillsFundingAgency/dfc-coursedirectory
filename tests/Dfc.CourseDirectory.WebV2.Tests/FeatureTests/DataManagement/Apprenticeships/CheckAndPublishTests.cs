using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Apprenticeships;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
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
        public async Task Get_ProviderHasNoApprenticeshipUploadAtProcessedStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ApprenticeshipUploadHasErrors_RedirectsToErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, _) = await TestData.CreateApprenticeshipUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should()
                .Be($"/data-upload/apprenticeships/errors?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var standard1 = await TestData.CreateStandard();
            var standard2 = await TestData.CreateStandard();

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddValidRow(standard1.StandardCode, standard1.Version);
                    rowBuilder.AddValidRow(standard2.StandardCode, standard2.Version);
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/apprenticeships/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("RowCount").TextContent.Should().Be("2");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_ProviderHasNoApprenticeshipUploadAtProcessedStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/check-publish?providerId={provider.ProviderId}")
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

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/check-publish?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to publish these apprenticeships");
        }

        [Fact]
        public async Task Post_ValidRequest_PublishesRowsAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var standard1 = await TestData.CreateStandard();
            var standard2 = await TestData.CreateStandard();

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddValidRow(standard1.StandardCode, standard1.Version);
                    rowBuilder.AddValidRow(standard2.StandardCode, standard2.Version);
                });

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/check-publish?providerId={provider.ProviderId}")
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
                .Should().Be($"/data-upload/apprenticeships/success?providerId={provider.ProviderId}");

            apprenticeshipUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetApprenticeshipUpload() { ApprenticeshipUploadId = apprenticeshipUpload.ApprenticeshipUploadId }));
            apprenticeshipUpload.UploadStatus.Should().Be(UploadStatus.Published);
            apprenticeshipUpload.PublishedOn.Should().Be(Clock.UtcNow);

            var journeyInstance = GetJourneyInstance<PublishJourneyModel>(
                "PublishApprenticeshipUpload",
                keys => keys.With("providerId", provider.ProviderId));
            journeyInstance.State.ApprenticeshipsPublished.Should().Be(2);
        }
    }
}
