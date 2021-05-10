using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Venues
{
    public class PublishedTests : MvcTestBase
    {
        public PublishedTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var journeyInstance = CreateJourneyInstance(
                "PublishVenueUpload",
                keys => keys.With("providerId", provider.ProviderId),
                new PublishJourneyModel() { VenuesPublished = 3 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/venues/success?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("PublishedCount").TextContent.Should().Be("3");
        }
    }
}
