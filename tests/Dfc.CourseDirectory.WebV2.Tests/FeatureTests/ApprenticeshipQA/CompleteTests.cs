using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class CompleteTests : MvcTestBase
    {
        public CompleteTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_ProviderUserCannotAccess(TestUserType testUserType)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var apprenticeshipId = await TestData.CreateApprenticeship(
                ukprn,
                apprenticeshipTitle: "App title");

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: true,
                passed: true,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsTestUser(testUserType, providerId);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{providerId}/complete", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_ProviderDoesNotExistReturnsBadRequest()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{providerId}/complete", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.Submitted)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete | ApprenticeshipQAStatus.NotStarted)]
        public async Task Post_StatusNotInProgressReturnsBadRequest(ApprenticeshipQAStatus status)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: status);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var apprenticeshipId = await TestData.CreateApprenticeship(
                ukprn,
                apprenticeshipTitle: "App title");

            if (status != ApprenticeshipQAStatus.NotStarted)
            {
                var submissionId = await TestData.CreateApprenticeshipQASubmission(
                    providerId,
                    submittedOn: Clock.UtcNow,
                    submittedByUserId: providerUserId,
                    providerMarketingInformation: "The overview",
                    apprenticeshipIds: new[] { apprenticeshipId });

                if (status != ApprenticeshipQAStatus.Submitted && !status.HasFlag(ApprenticeshipQAStatus.UnableToComplete))
                {
                    var passed = status == ApprenticeshipQAStatus.Failed ? false : true;

                    await TestData.UpdateApprenticeshipQASubmission(
                        submissionId,
                        providerAssessmentPassed: passed,
                        apprenticeshipAssessmentsPassed: passed,
                        passed: passed,
                        lastAssessedByUserId: User.UserId,
                        lastAssessedOn: Clock.UtcNow);
                }
            }

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{providerId}/complete", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_SubmissionOutcomeNotKnownReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var apprenticeshipId = await TestData.CreateApprenticeship(
                ukprn,
                apprenticeshipTitle: "App title");

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{providerId}/complete", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(true, ApprenticeshipQAStatus.Passed)]
        [InlineData(false, ApprenticeshipQAStatus.Failed)]
        public async Task Post_ValidSetsCorrectStatusAndRequestRendersExpectedOutput(
            bool passed,
            ApprenticeshipQAStatus expectedStatus)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var apprenticeshipId = await TestData.CreateApprenticeship(
                ukprn,
                apprenticeshipTitle: "App title");

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: passed,
                apprenticeshipAssessmentsPassed: passed,
                passed: passed,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{providerId}/complete", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var providerName = doc.GetElementById("pttcd-apprenticeship-qa-complete-provider-name").TextContent.Trim();
            Assert.Equal("Provider 1", providerName);

            var newStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetProviderApprenticeshipQAStatus()
            {
                ProviderId = providerId
            }));
            Assert.Equal(expectedStatus, newStatus);
        }
    }
}
