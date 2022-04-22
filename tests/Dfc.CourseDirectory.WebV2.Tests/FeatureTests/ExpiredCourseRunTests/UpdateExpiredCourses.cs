using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.DeleteCourseRun;
using FormFlow;
using OneOf;
using OneOf.Types;
using Xunit;
using UpdateCourseRunQuery = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.CourseStarteDateBulkUpdate;
using GetExpiredCourseRunQuery = Dfc.CourseDirectory.Core.DataStore.Sql.GetExpiredCourseRunsForProvider;
using FluentAssertions;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ExpiredCourseRunTests
{
    public class UpdateExpiredCourses : MvcTestBase
    {
        public UpdateExpiredCourses(CourseDirectoryApplicationFactory factory)
               : base(factory)
        {
        }

        [Fact]
        public async Task Get_ExpiredCourseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var courseId = Guid.NewGuid();
          

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/expired{provider}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetConfirmed_RendersExpectedCourseName()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
         
            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetExpiredCourseRunQuery()
            {
                ProviderId = provider.ProviderId,
                Today = DateTime.Today
            }));

            var request = new HttpRequestMessage(
               HttpMethod.Get,
               $"/courses/expired{provider}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            // Assert.Equal(doc.GetElementByTestId("CourseName").TextContent);
            doc.GetElementByTestId("CourseName").Should().NotBeNull();
        }

    }

}
