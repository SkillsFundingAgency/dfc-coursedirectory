using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using Provider = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models.Provider;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class CourseRunDetailTests : TestBase
    {
        public CourseRunDetailTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Handle_WithCourseIdNotFound_ReturnsNotFound()
        {
            var courseId = Guid.NewGuid();
            var courseRunId = Guid.NewGuid();

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetCourse>()))
                .ReturnsAsync((Course)null);

            var result = await HttpClient.GetAsync(CourseRunDetailUrl(courseId, courseRunId));

            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task Handle_WithCourseRunIdNotFound_ReturnsNotFound()
        {
            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                CourseRuns = Array.Empty<CourseRun>()
            };

            var courseRunId = Guid.NewGuid();

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetCourse>()))
                .ReturnsAsync(course);

            var result = await HttpClient.GetAsync(CourseRunDetailUrl(course.CourseId, courseRunId));

            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task Handle_WithValidCourseIdAndCourseRunId_ReturnsOkWithResult()
        {
            var venue = new Venue
            {
                VenueId = Guid.NewGuid(),
                VenueName = "TestVenueName"
            };

            var lars = new Core.Search.Models.Lars
            {
                LearnAimRef = "00112233",
                LearnAimRefTitle = "TestLearnAimRefTitle"
            };

            var courseRun = new CourseRun
            {
                CourseRunId = Guid.NewGuid(),
                CourseRunStatus = CourseStatus.Live,
                DurationUnit = CourseDurationUnit.Months,
                DurationValue = 3,
                VenueId = venue.VenueId
            };

            var alternativeCourseRun = new CourseRun
            {
                CourseRunId = Guid.NewGuid(),
                CourseRunStatus = CourseStatus.Live,
                DurationUnit = CourseDurationUnit.Months,
                DurationValue = 3,
                VenueId = venue.VenueId
            };

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                ProviderUkprn = 12345678,
                LearnAimRef = lars.LearnAimRef,
                CourseRuns = new[]
                {
                    courseRun,
                    alternativeCourseRun
                }
            };

            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                UnitedKingdomProviderReferenceNumber = "12345678",
                ProviderContact = new[]
                {
                    new Core.DataStore.CosmosDb.Models.ProviderContact
                    {
                        ContactEmail = "test@test.com",
                        ContactType = "P"
                    }
                }
            };

            var sqlProvider = new Core.DataStore.Sql.Models.Provider
            {
                ProviderId = provider.Id,
                ProviderName = "TestProviderAlias",
                DisplayNameSource = ProviderDisplayNameSource.ProviderName
            };

            var feChoice = new Core.DataStore.CosmosDb.Models.FeChoice
            {
                UKPRN = course.ProviderUkprn,
                EmployerSatisfaction = 1.2M,
                LearnerSatisfaction = 3.4M
            };

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetCourse>()))
                .ReturnsAsync(course);

            CosmosDbQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetProviderByUkprn>()))
                .ReturnsAsync(provider);

            LarsSearchClient.Setup(s => s.Search(It.IsAny<LarsLearnAimRefSearchQuery>()))
                .ReturnsAsync(new SearchResult<Core.Search.Models.Lars>
                {
                    Items = new[] { new SearchResultItem<Core.Search.Models.Lars> { Record = lars } }
                });

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetVenuesByProvider>()))
                .ReturnsAsync(new[] { venue });

            CosmosDbQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<GetFeChoiceForProvider>()))
                .ReturnsAsync(feChoice);

            SqlQueryDispatcher.Setup(s => s.ExecuteQuery(It.IsAny<Core.DataStore.Sql.Queries.GetProviderById>()))
                .ReturnsAsync(sqlProvider);

            var result = await HttpClient.GetAsync(CourseRunDetailUrl(course.CourseId, courseRun.CourseRunId));

            result.StatusCode.Should().Be(StatusCodes.Status200OK);

            var resultJson = JObject.Parse(await result.Content.ReadAsStringAsync());

            using (new AssertionScope())
            {
                resultJson.ToObject<object>().Should().NotBeNull();
                resultJson["provider"].ToObject<object>().Should().NotBeNull();
                resultJson["provider"]["ukprn"].ToObject<int>().Should().Be(provider.Ukprn);
                resultJson["provider"]["providerName"].ToObject<string>().Should().Be(sqlProvider.DisplayName);
                resultJson["provider"]["tradingName"].ToObject<string>().Should().Be(sqlProvider.DisplayName);
                resultJson["provider"]["email"].ToObject<string>().Should().Be(provider.ProviderContact.Single().ContactEmail);
                resultJson["provider"]["learnerSatisfaction"].ToObject<decimal>().Should().Be(feChoice.LearnerSatisfaction);
                resultJson["provider"]["employerSatisfaction"].ToObject<decimal>().Should().Be(feChoice.EmployerSatisfaction);
                resultJson["course"].ToObject<object>().Should().NotBeNull();
                resultJson["course"]["courseId"].ToObject<Guid>().Should().Be(course.CourseId);
                resultJson["venue"].ToObject<object>().Should().NotBeNull();
                resultJson["venue"]["venueName"].ToObject<string>().Should().Be(venue.VenueName);
                resultJson["qualification"].ToObject<object>().Should().NotBeNull();
                resultJson["qualification"]["learnAimRef"].ToObject<string>().Should().Be(lars.LearnAimRef);
                resultJson["qualification"]["learnAimRefTitle"].ToObject<string>().Should().Be(lars.LearnAimRefTitle);
                resultJson["alternativeCourseRuns"][0]["courseRunId"].ToObject<Guid>().Should().Be(alternativeCourseRun.CourseRunId);
                resultJson["courseRunId"].ToObject<Guid>().Should().Be(courseRun.CourseRunId);
            }
        }

        private string CourseRunDetailUrl(Guid courseId, Guid courseRunId) => $"courserundetail?courseId={courseId}&courseRunId={courseRunId}";
    }
}
