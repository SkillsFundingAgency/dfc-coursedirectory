using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
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
        public async Task Get_ProviderHasNoVenueUploadAtProcessedStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_VenueUploadHasErrors_RedirectsToErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, _) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should()
                .Be($"/data-upload/venues/errors?providerId={provider.ProviderId}");
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var existingVenue = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var (venueUpload, _) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record => record.IsSupplementary = false);
                    rowBuilder.AddRow(record => record.IsSupplementary = false);

                    rowBuilder.AddRow(record =>
                    {
                        record.IsSupplementary = true;
                        record.VenueId = existingVenue.VenueId;
                    });
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("UploadedRowCount").TextContent.Should().Be("2");
                doc.GetElementByTestId("SupplementaryRowCount").TextContent.Should().Be("1");
                doc.GetElementByTestId("TotalRowCount").TextContent.Should().Be("3");
            }
        }

        [Fact]
        public async Task Get_VenueUploadHasNoSupplementaryRows_DoesNotRenderSupplementaryRowsUI()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, _) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record => record.IsSupplementary = false);
                    rowBuilder.AddRow(record => record.IsSupplementary = false);
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("SupplementaryRowsBlock").Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedWithErrors)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_ProviderHasNoVenueUploadAtProcessedStatus_ReturnsError(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateVenueUpload(provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}")
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

            var (venueUpload, _) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to publish these venues");
        }

        [Fact]
        public async Task Post_ValidRequest_PublishesRowsAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var (venueUpload, _) = await TestData.CreateVenueUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedSuccessfully,
                rowBuilder => rowBuilder.AddValidRows(3));

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/venues/check-publish?providerId={provider.ProviderId}")
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
                .Should().Be($"/data-upload/venues/success?providerId={provider.ProviderId}");

            venueUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUpload.VenueUploadId }));
            venueUpload.UploadStatus.Should().Be(UploadStatus.Published);
            venueUpload.PublishedOn.Should().Be(Clock.UtcNow);

            var journeyInstance = GetJourneyInstance<PublishJourneyModel>(
                "PublishVenueUpload",
                keys => keys.With("providerId", provider.ProviderId));
            journeyInstance.State.VenuesPublished.Should().Be(3);
        }
    }
}
