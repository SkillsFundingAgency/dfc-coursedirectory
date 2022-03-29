using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ExpiredCourseRuns
{
    public class ExpiredCourseRunsTests : MvcTestBase
    {
        public ExpiredCourseRunsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        //------------------------------------------------------
        // These unit tests need to be updated accordingly.
        // This was just put here as a template to get it
        // to deploy to dev correctly.
        //------------------------------------------------------

        [Fact]
        public async Task Get_ReturnsExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var course1 = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: builder =>
                {
                    builder.WithCourseRun(startDate: Clock.UtcNow.Date.AddDays(-1));  // Expired
                });

            var course2 = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: builder =>
                {
                    builder.WithCourseRun(startDate: Clock.UtcNow.Date.AddDays(1));  // Not expired
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/expired?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }
    }
}
