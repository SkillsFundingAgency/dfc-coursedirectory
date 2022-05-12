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
using GovUk.Frontend.AspNetCore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ExpiredCourseRunTests
{
    public class UpdateExpiredCoursesTests : MvcTestBase
    {
        public UpdateExpiredCoursesTests(CourseDirectoryApplicationFactory factory)
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
            response.StatusCode.Should().Be(HttpStatusCode.OK);
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
                rows[0].GetElementByTestId("DeliveryMode").TextContent.Should().Be(course1.CourseRuns.Single().DeliveryMode.ToString(""));
                //  rows[0].GetElementByTestId("ProviderCourseRef").TextContent.Should().Be(course1.CourseRuns.Single().ProviderCourseId);
                rows[0].GetElementByTestId("StartDate").TextContent.Should().Be(course1.CourseRuns.Single().StartDate.Value.ToString("dd/MM/yyyy"));
            }
        }

        /*
        [Fact]
        private async Task Post_CheckBoxes_ReturnsErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetExpiredCourseRunQuery()
            {
                ProviderId = provider.ProviderId,
              
            }));



            // Act
            var updateCourseStartDate = new HttpRequestMessage(HttpMethod.Post, $"/courses/expired?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                 
                    .Add("selectedCourses", new List<Guid> { course.CourseId })
                    .ToContent()
            };
            // Act

            var postCourseRunResponse = await HttpClient.SendAsync(updateCourseStartDate);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("selectedCourses", "Select a course to update");

            }
        }  */


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
            }));

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
                // rows[0].GetElementByTestId("ProviderCourseRef").TextContent.Should().Be(course.CourseRuns.Single().ProviderCourseId);
                rows[0].GetElementByTestId("StartDate").TextContent.Should().Be(course.CourseRuns.Single().StartDate.Value.ToString("dd/MM/yyyy"));
            }
        } 

      

        [Fact]
        private async Task Post_NewStartedate()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);


            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpdateCourseRunQuery()
            {
                ProviderId = provider.ProviderId,
                StartDate = DateTime.Today
            }));


            // Act
            var updateCourseStartDate = new HttpRequestMessage(HttpMethod.Post,
                $"/courses/expired/updated?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("NewStartDate.Year", "2023").Add("NewStartDate.Month", "01").Add("NewStartDate.Day", "01")
                    .Add("selectedRows", new List<Guid> { course.CourseId })
                    .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(updateCourseStartDate);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Fact]
        private async Task Post_NewStartedateWithNoRows()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);


            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpdateCourseRunQuery()
            {
                ProviderId = provider.ProviderId,
                StartDate = DateTime.Today
            }));


            // Act
            var updateCourseStartDate = new HttpRequestMessage(HttpMethod.Post,
                $"/courses/expired/updated?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("NewStartDate.Year", "2023").Add("NewStartDate.Month", "01").Add("NewStartDate.Day", "01")
                    .ToContent()
            };
            var postCourseRunResponse = await HttpClient.SendAsync(updateCourseStartDate);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact]
        private async Task Post_StartDateUpdateDateinThePast_ReturnsErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpdateCourseRunQuery()
            {
                ProviderId = provider.ProviderId,
                StartDate = DateTime.Today
            }));



            // Act
            var updateCourseStartDate = new HttpRequestMessage(HttpMethod.Post, $"/courses/expired/updated?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder().
                    Add("NewStartDate.Year", "2020").Add("NewStartDate.Month", "01").Add("NewStartDate.Day", "01")
                    .Add("selectedRows", new List<Guid> { course.CourseId })
                    .ToContent()
            };
            // Act
 
            var postCourseRunResponse = await HttpClient.SendAsync(updateCourseStartDate);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("NewStartDate", "Start date cannot be earlier than today's date");
     
            }
        }

        [Fact]
        private async Task Post_StartDateUpdateEmpty_ReturnsErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(TestUserType.ProviderSuperUser, provider.ProviderId);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpdateCourseRunQuery()
            {
                ProviderId = provider.ProviderId,
                StartDate = DateTime.Today
            }));



            // Act
            var updateCourseStartDate = new HttpRequestMessage(HttpMethod.Post, $"/courses/expired/updated?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder().
                    Add("NewStartDate.Year", "").Add("NewStartDate.Month", "").Add("NewStartDate.Day", "")
                    .Add("selectedRows", new List<Guid> { course.CourseId })
                    .ToContent()
            };
            // Act

            var postCourseRunResponse = await HttpClient.SendAsync(updateCourseStartDate);

            // Assert
            postCourseRunResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var doc = await postCourseRunResponse.GetDocument();
            using (new AssertionScope())
            {
                doc.AssertHasError("NewStartDate", "Enter start date");

            }
        }


    }

}
