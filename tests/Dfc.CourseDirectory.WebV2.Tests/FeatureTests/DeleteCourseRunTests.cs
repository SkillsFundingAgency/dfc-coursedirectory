using System;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Features.DeleteCourseRun;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Moq;
using OneOf;
using OneOf.Types;
using Xunit;
using DeleteCourseRunQuery = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries.DeleteCourseRun;

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
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo());

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.ClassroomBased,
                    CourseStudyMode.FullTime,
                    CourseAttendancePattern.Daytime,
                    venueId: venueId));

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.Online,
                    CourseStudyMode.PartTime,
                    CourseAttendancePattern.Evening,
                    venueId: null,
                    national: true));

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.Online,
                    CourseStudyMode.PartTime,
                    CourseAttendancePattern.Evening,
                    national: true,
                    providerCourseId: "My course"));

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.Online,
                    CourseStudyMode.PartTime,
                    CourseAttendancePattern.Evening,
                    national: true,
                    providerCourseId: null));

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.Online,
                    CourseStudyMode.PartTime,
                    CourseAttendancePattern.Evening,
                    national: true,
                    flexibleStartDate: true));

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

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
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo(),
                configureCourseRuns: b => b.WithCourseRun(
                    CourseDeliveryMode.Online,
                    CourseStudyMode.PartTime,
                    CourseAttendancePattern.Evening,
                    national: true,
                    startDate: new DateTime(2020, 4, 1)));

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2Fcourses");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("01/04/2020", doc.GetSummaryListValueWithKey("Start date"));
        }

        [Fact]
        public async Task Get_ValidRequest_RendersCancelLinkFromReturnUrlQueryParameter()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var course = await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());
            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var returnUrl = "/courses";

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl={UrlEncoder.Default.Encode(returnUrl)}");

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
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(course.Id, courseRunId);

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
            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            CreateJourneyInstance(course.Id, courseRunId);

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
            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(course.Id, courseRunId);

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

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo());

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete?returnUrl=%2fcourses")
            {
                Content = requestContent
            };

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(course.Id, courseRunId);

            DeleteCourseRunQuery capturedDeleteCourseRunQuery = null;
            CosmosDbQueryDispatcher.Callback<DeleteCourseRunQuery, OneOf<NotFound, Success>>(q => capturedDeleteCourseRunQuery = q);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);

            Assert.Equal(
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete/confirmed",
                response.Headers.Location.OriginalString);

            CosmosDbQueryDispatcher.Verify(d => d.ExecuteQuery(It.IsAny<DeleteCourseRunQuery>()), Times.Once);
            using (new AssertionScope())
            {
                capturedDeleteCourseRunQuery.Should().NotBeNull();
                capturedDeleteCourseRunQuery.CourseId.Should().Be(course.Id);
                capturedDeleteCourseRunQuery.CourseRunId.Should().Be(courseRunId);
                capturedDeleteCourseRunQuery.ProviderUkprn.Should().Be(provider.Ukprn);
                capturedDeleteCourseRunQuery.UpdatedBy.Should().Be(TestUserInfo.DefaultUserId);
                capturedDeleteCourseRunQuery.UpdatedDate.Should().Be(MutableClock.Start);
            }
        }

        [Fact]
        public async Task GetConfirmed_RendersExpectedCourseName()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo());

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            await CosmosDbQueryDispatcher.Object.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = course.Id,
                CourseRunId = courseRunId,
                ProviderUkprn = provider.Ukprn
            });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete/confirmed");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(
                course.Id,
                courseRunId,
                new JourneyModel()
                {
                    CourseName = "Maths",
                    ProviderId = provider.ProviderId,
                    ProviderUkprn = provider.Ukprn
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

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo());

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            await CosmosDbQueryDispatcher.Object.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = course.Id,
                CourseRunId = courseRunId,
                ProviderUkprn = provider.Ukprn
            });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete/confirmed");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(
                course.Id,
                courseRunId,
                new JourneyModel()
                {
                    CourseName = "Maths",
                    ProviderId = provider.ProviderId,
                    ProviderUkprn = provider.Ukprn
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

            var course = await TestData.CreateCourse(
                provider.ProviderId,
                qualificationCourseTitle: "Maths",
                createdBy: User.ToUserInfo());

            var courseRunId = await GetCourseRunIdForCourse(course.Id);

            // Create another live course
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo());

            await CosmosDbQueryDispatcher.Object.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = course.Id,
                CourseRunId = courseRunId,
                ProviderUkprn = provider.Ukprn
            });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/courses/{course.Id}/course-runs/{courseRunId}/delete/confirmed");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            CreateJourneyInstance(
                course.Id,
                courseRunId,
                new JourneyModel()
                {
                    CourseName = "Maths",
                    ProviderId = provider.ProviderId,
                    ProviderUkprn = provider.Ukprn
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

        private async Task<Guid> GetCourseRunIdForCourse(Guid courseId)
        {
            var course = await CosmosDbQueryDispatcher.Object.ExecuteQuery(
                new GetCourseById()
                {
                    CourseId = courseId
                });

            var courseRun = Assert.Single(course.CourseRuns);

            return courseRun.Id;
        }
    }
}
