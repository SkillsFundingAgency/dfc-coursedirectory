using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class HidePassedNotificationTests : MvcTestBase
    {
        public HidePassedNotificationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }


        [Fact]
        public async Task Post_FeOnlyProviderReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_HelpdeskUserReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidSubmissionReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotStartedReturnsBadRequest()
        {
            // Arrange
            var providerName = "Provider 1";

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.Apprenticeships);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_FailedQAReturnsBadRequest()
        {
            // Arrange
            var providerName = "Provider 1";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: providerUser)).ApprenticeshipId;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: false,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.SpecificEmployerNamed,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded);

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: false,
                passed: false,
                lastAssessedByUserId: adminUser.UserId,
                lastAssessedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.Failed);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_SubmittedQAStatusReturnsBadRequest()
        {
            // Arrange
            var providerName = "Provider 1";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: providerUser)).ApprenticeshipId;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.Submitted);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task Post_InProgressQAReturnsBadRequest()
        {
            // Arrange
            var providerName = "Provider 1";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: providerUser)).ApprenticeshipId;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: false,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.SpecificEmployerNamed,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded);

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: adminUser.UserId,
                lastAssessedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.InProgress);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_UnableToCompleteQAStatusReturnsBadRequest()
        {
            // Arrange
            var providerName = "Provider 1";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.UnableToComplete,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: providerUser)).ApprenticeshipId;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: false,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.SpecificEmployerNamed,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded);

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: false,
                passed: false,
                lastAssessedByUserId: adminUser.UserId,
                lastAssessedOn: Clock.UtcNow);

            await TestData.CreateApprenticeshipQAUnableToCompleteInfo(
                provider.ProviderId,
                ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision,
                comments: "",
                addedByUserId: adminUser.UserId,
                addedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(
                provider.ProviderId,
                ApprenticeshipQAStatus.InProgress | ApprenticeshipQAStatus.UnableToComplete);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ValidRequestReturnsRedirect()
        {
            // Arrange
            var providerName = "Provider 1";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: providerUser)).ApprenticeshipId;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: false,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.SpecificEmployerNamed,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded);

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: true,
                passed: true,
                lastAssessedByUserId: adminUser.UserId,
                lastAssessedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.Passed);

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Post_AlreadyHiddenReturnsBadRequest()
        {
            // Arrange
            var providerName = "Provider 1";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;

            var provider = await TestData.CreateProvider(
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);
            var adminUser = await TestData.CreateUser();

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: providerUser)).ApprenticeshipId;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUser.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: false,
                complianceComments: null,
                ApprenticeshipQAProviderComplianceFailedReasons.SpecificEmployerNamed,
                stylePassed: true,
                styleComments: null,
                ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded);

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: true,
                passed: true,
                lastAssessedByUserId: adminUser.UserId,
                lastAssessedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(provider.ProviderId, ApprenticeshipQAStatus.Passed);

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new UpdateHidePassedNotification()
                {
                    ApprenticeshipQASubmissionId = submissionId,
                    HidePassedNotification = true
                }));

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={provider.ProviderId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
