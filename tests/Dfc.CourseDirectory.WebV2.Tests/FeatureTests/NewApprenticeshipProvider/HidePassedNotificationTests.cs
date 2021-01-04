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
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await User.AsProviderUser(providerId, ProviderType.FE);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_HelpdeskUserReturnsForbidden()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidSubmissionReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NotStartedReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;
            var providerName = "Provider 1";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.Apprenticeships);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_FailedQAReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: providerUser)).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
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

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Failed);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_SubmittedQAStatusReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: providerUser)).Id;

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Submitted);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task Post_InProgressQAReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: providerUser)).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
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

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.InProgress);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_UnableToCompleteQAStatusReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.UnableToComplete,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: providerUser)).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
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
                providerId,
                ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision,
                comments: "",
                addedByUserId: adminUserId,
                addedOn: Clock.UtcNow);

            await TestData.SetProviderApprenticeshipQAStatus(
                providerId,
                ApprenticeshipQAStatus.InProgress | ApprenticeshipQAStatus.UnableToComplete);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ValidRequestReturnsRedirect()
        {
            // Arrange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: providerUser)).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
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

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Passed);

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Post_AlreadyHiddenReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.Apprenticeships);

            var providerUser = await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(providerId, standard, createdBy: providerUser)).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: adminUserId,
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

            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Passed);

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new UpdateHidePassedNotification()
                {
                    ApprenticeshipQASubmissionId = submissionId,
                    HidePassedNotification = true
                }));

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
