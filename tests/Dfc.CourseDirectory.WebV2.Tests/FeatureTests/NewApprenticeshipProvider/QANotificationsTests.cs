using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class QANotificationsTests : MvcTestBase
    {
        public QANotificationsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted, "pttcd-new-apprenticeship-provider__qa-notifications-not-started")]
        [InlineData(ApprenticeshipQAStatus.Submitted, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.InProgress, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Failed, "pttcd-new-apprenticeship-provider__qa-notifications-failed")]
        public async Task RendersCorrectMessage(ApprenticeshipQAStatus qaStatus, string expectedNotificationId)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var notificationElements = doc.QuerySelectorAll("div")
                .Where(e => e.Id?.StartsWith("pttcd-new-apprenticeship-provider__qa-notifications-") ?? false);

            if (expectedNotificationId == null)
            {
                Assert.Empty(notificationElements);
            }
            else
            {
                var notification = Assert.Single(notificationElements);
                Assert.Equal(expectedNotificationId, notification.Id);
            }
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted, "pttcd-new-apprenticeship-provider__qa-notifications-not-started")]
        [InlineData(ApprenticeshipQAStatus.Submitted, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.InProgress, "pttcd-new-apprenticeship-provider__qa-notifications-submitted")]
        [InlineData(ApprenticeshipQAStatus.Failed, "pttcd-new-apprenticeship-provider__qa-notifications-failed")]
        [InlineData(ApprenticeshipQAStatus.Passed, "pttcd-new-apprenticeship-provider__qa-notifications-passed")]
        public async Task FEOnlyProviderRendersNoMessage(ApprenticeshipQAStatus qaStatus, string notificationId)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: qaStatus,
                providerType: ProviderType.FE);

            await User.AsProviderUser(providerId, ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById(notificationId));
        }

        [Fact]

        public async Task PassedQANotification_Is_Not_Visible_Once_Notification_Is_Closed()
        {
            // Arrange
            var ukprn = 12345;
            var adminUserId = $"admin-user";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var passedProviderAssessmentOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
                assessedOn: passedProviderAssessmentOn,
                compliancePassed: true,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.None);

            // Update QA submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: true,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Passed);
            await TestData.UpdateHidePassedNotification(submissionId, true);
            await User.AsProviderSuperUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var notificationElements = doc.QuerySelectorAll("div")
                .Where(e => e.Id == "pttcd-new-apprenticeship-provider__qa-notifications-passed");

            Assert.Empty(notificationElements);
        }



        [Fact]

        public async Task PassedQANotification_Is_Visible_For_PassedQAProviders()
        {
            // Arrange
            var ukprn = 12345;
            var adminUserId = $"admin-user";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var passedProviderAssessmentOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
                assessedOn: passedProviderAssessmentOn,
                compliancePassed: true,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.None);

            // Update QA submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: true,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Passed);
            await User.AsProviderSuperUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var notificationElements = doc.QuerySelectorAll("div")
                .Where(e => e.Id == "pttcd-new-apprenticeship-provider__qa-notifications-passed");

            Assert.NotEmpty(notificationElements);
        }

        [Fact]

        public async Task PassedQANotification_Is_Not_Visible_For_MigratedPassedQAProviders()
        {
            // Arrange
            var ukprn = 12345;
            var adminUserId = $"admin-user";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            await User.AsProviderSuperUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync("QANotificationsTests");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var notificationElements = doc.QuerySelectorAll("div")
                .Where(e => e.Id == "pttcd-new-apprenticeship-provider__qa-notifications-passed");

            Assert.Empty(notificationElements);
        }

    }

    public class QANotificationsTestsController : Controller
    {
        [HttpGet("QANotificationsTests")]
        public IActionResult Get() => View("~/FeatureTests/NewApprenticeshipProvider/QANotificationsTests.cshtml");
    }
}
