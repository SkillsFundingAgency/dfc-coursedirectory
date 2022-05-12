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
using DeleteCourseRunQuery = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.DeleteCourseRun;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests
{
    public class DeleteCourseRunTests : MvcTestBase
    {
        public DeleteCourseRunTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_CourseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{courseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_CourseRunDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessCourse_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider();

            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: builder => builder.WithCourseRun(courseName: "Maths"));

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Maths", doc.GetSummaryListValueWithKey("Course name"));
        }

        [Fact]
        public async Task Get_ValidRequestCourseRunWithVenue_RendersLocationRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue")).VenueId;

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithClassroomBasedCourseRun(venueId: venueId));

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Test Venue", doc.GetSummaryListValueWithKey("Venue"));
        }

        [Fact]
        public async Task Get_ValidRequestCourseRunWithNoVenue_DoesNotRenderLocationRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue");

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithWorkBasedCourseRun());

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetSummaryListValueWithKey("Course location"));
        }

        [Fact]
        public async Task Get_ValidRequestCourseRunWithProviderCourseId_RendersYourReferenceRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue");

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(providerCourseRef: "My course"));

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("My course", doc.GetSummaryListValueWithKey("Your reference"));
        }

        [Fact]
        public async Task Get_ValidRequestCourseRunWithNoProviderCourseId_DoesNotRenderYourReferenceRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue");

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(providerCourseRef: null));

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetSummaryListValueWithKey("Your reference"));
        }

        [Fact]
        public async Task Get_ValidRequestCourseRunWithFlexibleStartDate_RendersFlexibleStartDateRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue");

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(flexibleStartDate: true));

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Flexible", doc.GetSummaryListValueWithKey("Start date"));
        }

        [Fact]
        public async Task Get_ValidRequestCourseRunWithSpecificStartDate_RendersStartDateRow()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue");

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithOnlineCourseRun(startDate: new DateTime(2020, 4, 1)));

            // Date formatting patch to support multiple configs
            var nulldate = new DateTime();
            string s; //separator
            const string enDash = "-";
            const string fSlash = "/";
            if (nulldate.ToString().Contains("-")) 
            { s = enDash; } 
            else { s = fSlash; }

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal($"01{s}04{s}2020", doc.GetSummaryListValueWithKey("Start date"));
        }

        [Fact]
        public async Task Get_ValidRequest_RendersCancelLinkFromReturnUrlQueryParameter()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var returnUrl = "/courses";

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl={UrlEncoder.Default.Encode(returnUrl)}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal(returnUrl, doc.GetElementByTestId("cancel-btn").GetAttribute("href"));
        }

        [Fact]
        public async Task Post_CourseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{courseId}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(courseId, courseRunId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_CourseRunDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(course.CourseId, courseRunId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessCourse_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider();

            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            CreateJourneyInstance(course.CourseId, courseRunId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotConfirmed_ReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(course.CourseId, courseRunId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete the course");
        }

        [Fact]
        public async Task Post_ValidRequest_DeletesCourseRunAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(course.CourseId, courseRunId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);

            Assert.Equal(
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete/confirmed",
                response.Headers.Location.OriginalString);

            SqlQuerySpy.VerifyQuery<DeleteCourseRunQuery, OneOf<NotFound, Success>>(q =>
                q.CourseId == course.CourseId &&
                q.CourseRunId == courseRunId &&
                q.DeletedBy.UserId == User.ToUserInfo().UserId &&
                q.DeletedOn == Clock.UtcNow);
        }

        [Fact]
        public async Task GetConfirmed_RendersExpectedCourseName()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery(learnAimRefTitle: "Maths")).LearnAimRef;

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                learnAimRef);

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = course.CourseId,
                CourseRunId = courseRunId,
                DeletedBy = User.ToUserInfo(),
                DeletedOn = Clock.UtcNow
            }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete/confirmed");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(
                course.CourseId,
                courseRunId,
                new JourneyModel()
                {
                    CourseName = "Maths",
                    ProviderId = provider.ProviderId
                });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Equal("Maths", doc.GetElementByTestId("CourseName").TextContent);
        }

        [Fact]
        public async Task GetConfirmed_NoOtherLiveCourseRuns_DoesNotRenderViewEditCopyLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery(learnAimRefTitle: "Maths")).LearnAimRef;

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                learnAimRef);

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = course.CourseId,
                CourseRunId = courseRunId,
                DeletedBy = User.ToUserInfo(),
                DeletedOn = Clock.UtcNow
            }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete/confirmed");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(
                course.CourseId,
                courseRunId,
                new JourneyModel()
                {
                    CourseName = "Maths",
                    ProviderId = provider.ProviderId
                });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementByTestId("ViewEditCopyDeleteLink"));
        }

        [Fact]
        public async Task GetConfirmed_HasOtherLiveCourseRuns_DoesRenderViewEditCopyLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery(learnAimRefTitle: "Maths")).LearnAimRef;

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                learnAimRef);

            var courseRunId = course.CourseRuns.Single().CourseRunId;

            // Create another live course
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = course.CourseId,
                CourseRunId = courseRunId,
                DeletedBy = User.ToUserInfo(),
                DeletedOn = Clock.UtcNow
            }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.CourseId}/course-runs/{courseRunId}/delete/confirmed");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(
                course.CourseId,
                courseRunId,
                new JourneyModel()
                {
                    CourseName = "Maths",
                    ProviderId = provider.ProviderId
                });

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementByTestId("ViewEditCopyDeleteLink"));
        }

        private JourneyInstance<JourneyModel> CreateJourneyInstance(
            Guid courseId,
            Guid courseRunId,
            JourneyModel flowModel = null)
        {
            return CreateJourneyInstance(
                "DeleteCourseRun",
                configureKeys: keysBuilder => keysBuilder
                    .With("courseId", courseId)
                    .With("courseRunId", courseRunId),
                flowModel ?? new JourneyModel());
        }
    }
}
