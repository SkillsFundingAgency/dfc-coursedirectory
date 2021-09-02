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

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Apprenticeships
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
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_NoUnpublishedApprenticeshipUpload_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            if (uploadStatus != null)
            {
                await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/delete?providerId={provider.ProviderId}")
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
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), UploadStatus.ProcessedSuccessfully);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete these apprenticeships");
        }

        [Theory]
        [InlineData(UploadStatus.ProcessedWithErrors)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        public async Task Post_ValidRequest_AbandonsApprenticeshipUploadAndRedirects(UploadStatus uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/apprenticeships/delete?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("Confirm", "true")
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/data-upload/apprenticeships/resolve/delete/success?providerId={provider.ProviderId}");

            SqlQuerySpy.VerifyQuery<SetApprenticeshipUploadAbandoned, OneOf<NotFound, Success>>(
                q => q.ApprenticeshipUploadId == apprenticeshipUpload.ApprenticeshipUploadId && q.AbandonedOn == Clock.UtcNow);
        }
    }
}
