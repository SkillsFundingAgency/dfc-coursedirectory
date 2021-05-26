using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class DeleteUploadTests : MvcTestBase
    {
        public DeleteUploadTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_NoUnpublishedVenueUpload_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus != null)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/delete?providerId={provider.ProviderId}")
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

            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedSuccessfully);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete these venues");
        }

        [Theory]
        [InlineData(UploadStatus.ProcessedWithErrors)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        public async Task Post_ValidRequest_AbandonsVenueUploadAndRedirects(UploadStatus uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, _) = await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/data-upload/venues/resolve/delete/success?providerId={provider.ProviderId}");

            SqlQuerySpy.VerifyQuery<SetVenueUploadAbandoned, OneOf<NotFound, Success>>(
                q => q.VenueUploadId == venueUpload.VenueUploadId && q.AbandonedOn == Clock.UtcNow);
        }
    }
}
