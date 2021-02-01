using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ProviderDashboard
{
    public class DashboardTests : MvcTestBase
    {
        public DashboardTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var ukprn = 12345;
            var providerName = "Test provider";

            var providerId = await TestData.CreateProvider(
                ukprn,
                providerName,
                providerType: ProviderType.Apprenticeships | ProviderType.FE | ProviderType.TLevels,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var venues = await CreateVenues(providerId, count: 2);

            await CreateCourses(providerId, count: 5);
            await CreateApprenticeships(providerId, count: 3);
            await CreateTLevels(providerId, tLevelDefinitions, venues, 4);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ukprn").TextContent.Should().Be(ukprn.ToString());
            doc.GetElementByTestId("provider-name").TextContent.Should().Be(providerName);

            doc.GetElementByTestId("courses-row").TextContent.Should().NotBeNull();
            doc.GetElementByTestId("apprenticeships-row").TextContent.Should().NotBeNull();
            doc.GetElementByTestId("tlevels-row").TextContent.Should().NotBeNull();

            doc.GetElementByTestId("course-count").TextContent.Should().Be("5");
            doc.GetElementByTestId("apprenticeship-count").TextContent.Should().Be("3");
            doc.GetElementByTestId("tlevel-count").TextContent.Should().Be("4");
            doc.GetElementByTestId("venue-count").TextContent.Should().Be("2");
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        public async Task HasNotPassedQA_DoesNotRenderApprenticeshipsRow(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                apprenticeshipQAStatus: qaStatus);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("apprenticeships-row").Should().BeNull();
        }

        [Fact]
        public async Task ProviderHasNoVenues_DoesNotRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("venues-view-edit-link").Should().BeNull();
        }

        [Fact]
        public async Task ProviderHasVenues_DoesRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            await CreateVenues(providerId, count: 1);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("venues-view-edit-link").Should().NotBeNull();
        }

        [Fact]
        public async Task FEProviderHasNoCourses_DoesNotRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("courses-view-edit-link").Should().BeNull();
        }

        [Fact]
        public async Task FEProviderHasCourses_DoesRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            await CreateCourses(providerId, count: 1);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("courses-view-edit-link").Should().NotBeNull();
        }

        [Fact]
        public async Task ApprenticeshipProviderHasNoApprenticeships_DoesNotRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("apprenticeships-view-edit-link").Should().BeNull();
        }

        [Fact]
        public async Task ApprenticeshipProviderHasApprenticeships_DoesRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            await CreateApprenticeships(providerId, count: 1);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("apprenticeships-view-edit-link").Should().NotBeNull();
        }

        [Fact]
        public async Task TLevelsProviderHasNoTLevels_DoesNotRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("tlevels-view-edit-link").Should().BeNull();
        }

        [Fact]
        public async Task TLevelsProviderHasTLevels_DoesRenderViewAndEditLink()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();
            var venues = await CreateVenues(providerId, count: 2);
            await CreateTLevels(providerId, tLevelDefinitions, venues, 3);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("tlevels-view-edit-link").Should().NotBeNull();
        }

        [Fact]
        public async Task Notifications_WithPastStartDateRunCountGreaterThanZero_DisplaysCourseStartDatesNotification()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE);

            await TestData.CreateCourse(
                    providerId,
                    createdBy: User.ToUserInfo(),
                    configureCourseRuns: courseRunBuilder =>
                        courseRunBuilder.WithCourseRun(
                            CourseDeliveryMode.ClassroomBased,
                            CourseStudyMode.FullTime,
                            CourseAttendancePattern.Daytime,
                            startDate: Clock.UtcNow.AddMonths(-1).Date));

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("courseStartDateNotification").Should().NotBeNull();
        }

        [Theory]
        [InlineData(CourseStatus.MigrationPending)]
        [InlineData(CourseStatus.MigrationReadyToGoLive)]
        public async Task Notifications_WithMigrationCourseStatusCourseRunCountGreaterThanZero_DisplaysMigrationNotification(CourseStatus courseStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE);

            await TestData.CreateCourse(
                    providerId,
                    createdBy: User.ToUserInfo(),
                    courseStatus: courseStatus,
                    configureCourseRuns: courseRunBuilder =>
                        courseRunBuilder.WithCourseRun(
                            CourseDeliveryMode.ClassroomBased,
                            CourseStudyMode.FullTime,
                            CourseAttendancePattern.Daytime));

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("migrationNotification").Should().NotBeNull();
        }

        [Fact]
        public async Task Notifications_WithBulkUploadInProgress_DisplaysProcessingCoursesBulkUploadNotification()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                bulkUploadInProgress: true);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("processingCoursesBulkUploadNotification").Should().NotBeNull();
        }

        [Fact]
        public async Task Notifications_WithBulkUploadErrorOrPendingCountGreaterThanZero_DisplaysCoursesBulkUploadErrorNotification()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE);

            await TestData.CreateCourse(
                    providerId,
                    createdBy: User.ToUserInfo(),
                    courseStatus: CourseStatus.BulkUploadPending,
                    configureCourseRuns: courseRunBuilder =>
                        courseRunBuilder.WithCourseRun(
                            CourseDeliveryMode.ClassroomBased,
                            CourseStudyMode.FullTime,
                            CourseAttendancePattern.Daytime));

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("coursesBulkUploadErrorNotification").Should().NotBeNull();
        }

        [Fact]
        public async Task Notification_WithBulkUploadReadyToGoLiveCourseRunCountGreaterThanZero_DisplaysCoursesBulkUploadSuccessfulNotification()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE);

            await TestData.CreateCourse(
                    providerId,
                    createdBy: User.ToUserInfo(),
                    courseStatus: CourseStatus.BulkUploadReadyToGoLive,
                    configureCourseRuns: courseRunBuilder =>
                        courseRunBuilder.WithCourseRun(
                            CourseDeliveryMode.ClassroomBased,
                            CourseStudyMode.FullTime,
                            CourseAttendancePattern.Daytime));

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("coursesBulkUploadSuccessfulNotification").Should().NotBeNull();
        }

        [Fact]
        public async Task Notification_WithBulkUploadReadyToGoLiveApprenticeshipsCountGreaterThanZero_DisplaysApprenticeshipsBulkUploadSuccessfulNotification()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships);

            var standard = await TestData.CreateStandard(123, 456, "TestStandard");

            await TestData.CreateApprenticeship(
                providerId,
                standard,
                User.ToUserInfo(),
                ApprenticeshipStatus.BulkUploadReadyToGoLive);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("apprenticeshipsBulkUploadSuccessfulNotification").Should().NotBeNull();
        }

        [Fact]
        public async Task ProviderTypeNone_RendersNewProviderMessage()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.None);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("NewProvider").Should().NotBeNull();
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.TLevels)]
        public async Task ProviderTypeNotNone_DoesNotRenderNewProviderMessage(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: providerType);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/dashboard?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("NewProvider").Should().BeNull();
        }

        private async Task CreateApprenticeships(Guid providerId, int count)
        {
            for (var i = 1; i <= count; i++)
            {
                var standardCode = 42 + i;
                var standardVersion = 1;

                var standard = await TestData.CreateStandard(
                    standardCode,
                    standardVersion,
                    standardName: $"Test standard {i}");

                await TestData.CreateApprenticeship(
                    providerId,
                    standard,
                    createdBy: User.ToUserInfo());
            }
        }

        private async Task<IReadOnlyCollection<Core.DataStore.CosmosDb.Models.Venue>> CreateVenues(Guid providerId, int count) =>
            await Task.WhenAll(Enumerable.Range(0, count).Select(i =>
                TestData.CreateVenue(providerId, venueName: $"Test {i}")));

        private async Task CreateCourses(Guid providerId, int count)
        {
            for (var i = 1; i <= count; i++)
            {
                await TestData.CreateCourse(
                    providerId,
                    createdBy: User.ToUserInfo(),
                    qualificationCourseTitle: $"Test {i}",
                    learnAimRef: $"TST{i}");
            }
        }

        private async Task CreateTLevels(Guid providerId, IEnumerable<Core.DataStore.Sql.Models.TLevelDefinition> tLevelDefinitions, IEnumerable<Core.DataStore.CosmosDb.Models.Venue> venues, int count) =>
            await Task.WhenAll(Enumerable.Range(0, count).Select(i =>
                TestData.CreateTLevel(
                    providerId,
                    tLevelDefinitions.OrderBy(_ => Guid.NewGuid()).First().TLevelDefinitionId,
                    new[] { venues.OrderBy(_ => Guid.NewGuid()).First().Id },
                    User.ToUserInfo(),
                    startDate: Clock.UtcNow.AddMonths(i).Date,
                    yourReference: $"YourReference{i}")));
    }
}
