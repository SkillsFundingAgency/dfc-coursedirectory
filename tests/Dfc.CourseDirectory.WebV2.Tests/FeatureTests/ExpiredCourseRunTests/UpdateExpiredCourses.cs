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
using GetSelectedExpiredCoursesQuery = Dfc.CourseDirectory.Core.DataStore.Sql.GetExpiredSelectedCourseRunsForProvider;

using FluentAssertions;
using FluentAssertions.Execution;

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


            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
             Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        }

        /*
        [Fact]
        public async Task Get_ExpiredCourseDoesNotExist_ReturnsFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/courses/expired?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        } */
        /*
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
        */

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
                rows[0].GetElementByTestId("DeliveryMode").TextContent.Should().Be(course1.CourseRuns.Single().DeliveryMode.ToString(""));
               
                rows[0].GetElementByTestId("StartDate").TextContent.Should().Be(course1.CourseRuns.Single().StartDate.Value.ToString("dd/MM/yyyy"));
            }
        }

       
       [Fact]
       public async Task GetConfirmed_RendersExpectedCourseName()
       {
           // Arrange
           var provider = await TestData.CreateProvider();

            var course = await TestData.CreateCourse(   
               provider.ProviderId,
               createdBy: User.ToUserInfo(),
               configureCourseRuns: builder =>
                 {
                      builder.WithCourseRun(startDate: Clock.UtcNow.Date.AddDays(-1));  // Expired
                });

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetSelectedExpiredCoursesQuery()
            {
                ProviderId = provider.ProviderId,
                Today = DateTime.Today,
            })) ;

           var request = new HttpRequestMessage(
              HttpMethod.Get,
              $"/courses/expired?providerId={provider.ProviderId}");

           await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

           // Act
           var response = await HttpClient.SendAsync(request);

           // Assert
           response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                var rows = doc.GetAllElementsByTestId("CourseRunRow");
                rows.Count.Should().Be(1);

                rows[0].GetElementByTestId("CourseName").TextContent.Should().Be(course.CourseRuns.Single().CourseName);
                rows[0].GetElementByTestId("DeliveryMode").TextContent.Should().Be(course.CourseRuns.Single().DeliveryMode.ToString(""));

                rows[0].GetElementByTestId("StartDate").TextContent.Should().Be(course.CourseRuns.Single().StartDate.Value.ToString("dd/MM/yyyy"));
            }


        } 
       

    }

}
