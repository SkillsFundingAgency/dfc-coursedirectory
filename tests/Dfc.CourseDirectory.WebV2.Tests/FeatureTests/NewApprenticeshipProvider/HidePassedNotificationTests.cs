using Dfc.CourseDirectory.WebV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class HidePassedNotificationTests : TestBase
    {
        public HidePassedNotificationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }


        [Fact]
        public async Task Post_FeOnlyProviderReturnsForbidden()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed, providerType: ProviderType.Apprenticeships);
            await User.AsProviderUser(providerId, ProviderType.FE);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
             Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_DeveloperUserReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed, providerType: ProviderType.Apprenticeships);
            await User.AsDeveloper();

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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed, providerType: ProviderType.Apprenticeships);
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
            var providerId = await TestData.CreateProvider(apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed, providerType: ProviderType.Apprenticeships);
            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/",null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.InProgress)]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete)]
        public async Task Post_NotPassedQAStatusReturnsBadRequest(ApprenticeshipQAStatus status)
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
                apprenticeshipQAStatus: status,
                providerType: ProviderType.Apprenticeships);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //update qa submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            //create provider assessment
            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            var PassedQAOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                adminUserId,
                Clock.UtcNow,
                false,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.InsufficientDetail,
                true, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.TermCourseUsed
                );

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
            var requestedOn = Clock.UtcNow;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus:  ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //update qa submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: true,
                passed: true,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            //create provider assessment
            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            var PassedQAOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                adminUserId,
                Clock.UtcNow,
                true,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                true, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.None
                );

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
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed, 
                providerType: ProviderType.Apprenticeships);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //update qa submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: true,
                passed: true,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);


            //create provider assessment
            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            var PassedQAOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                adminUserId,
                Clock.UtcNow,
                true,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                true, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.None
                );

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            var response2 = await HttpClient.PostAsync(
                $"new-apprenticeship-provider/hide-passed-notification?providerId={providerId}&returnUrl=/", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }
    }
}
