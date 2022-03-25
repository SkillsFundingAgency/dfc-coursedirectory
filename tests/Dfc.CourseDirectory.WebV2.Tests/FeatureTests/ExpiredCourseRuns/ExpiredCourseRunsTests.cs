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

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                var rows = doc.GetAllElementsByTestId("CourseRunRow");
                rows.Count.Should().Be(1);

                rows[0].GetElementByTestId("CourseName").TextContent.Should().Be(course1.CourseRuns.Single().CourseName);
                rows[0].GetElementByTestId("StartDate").TextContent.Should().Be(course1.CourseRuns.Single().StartDate.Value.ToString("dd/MM/yyyy"));
            }
        }
    }
}
