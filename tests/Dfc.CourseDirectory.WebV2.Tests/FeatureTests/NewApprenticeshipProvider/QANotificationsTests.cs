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
            var provider = await TestData.CreateProvider(apprenticeshipQAStatus: qaStatus);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

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
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: qaStatus,
                providerType: ProviderType.FE);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

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
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var passedProviderAssessmentOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
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

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.Passed);
            await TestData.UpdateHidePassedNotification(submissionId, true);
            await User.AsProviderSuperUser(provider.ProviderId, ProviderType.Apprenticeships);

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
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");
            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var passedProviderAssessmentOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
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

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.Passed);
            await User.AsProviderSuperUser(provider.ProviderId, ProviderType.Apprenticeships);

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
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            await User.AsProviderSuperUser(provider.ProviderId, ProviderType.Apprenticeships);

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
