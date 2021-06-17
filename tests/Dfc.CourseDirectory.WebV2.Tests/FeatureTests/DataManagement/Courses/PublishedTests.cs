using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
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
                "PublishCourseUpload",
                keys => keys.With("providerId", provider.ProviderId),
                new PublishJourneyModel() { CoursesPublished = 3 });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/success?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("PublishedCount").TextContent.Should().Be("3");
        }
    }
}
