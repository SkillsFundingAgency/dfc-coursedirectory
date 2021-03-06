﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class ProviderSelectedTests : MvcTestBase
    {
        public ProviderSelectedTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUserCannotAccess(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsTestUser(testUserType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_ProviderDoesNotExistReturnsBadRequest()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Provider 1", doc.QuerySelector("h1").TextContent);

            var firstApp = doc.GetAllElementsByTestId("apprenticeship-assessment").First();
            var firstAppLabel = firstApp.GetElementByTestId("apprenticeship-assessment-label").TextContent.Trim();
            Assert.Equal("Test Standard", firstAppLabel);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_ProviderAssessmentCompletedRendersBadge(bool passed)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: passed,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var providerElement = doc.GetElementByTestId("provider-assessment");
            var providerBadge = providerElement.GetElementsByClassName("govuk-tag").SingleOrDefault();
            Assert.NotNull(providerBadge);
        }

        [Fact]
        public async Task Get_ProviderAssessmentNotCompletedDoesNotRenderBadge()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: null,
                apprenticeshipAssessmentsPassed: true,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var providerElement = doc.GetElementByTestId("provider-assessment");
            var providerBadge = providerElement.GetElementsByClassName("govuk-tag").SingleOrDefault();
            Assert.Null(providerBadge);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_ApprenticeshipAssessmentCompletedRendersBadge(bool passed)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: null,
                apprenticeshipAssessmentsPassed: passed,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var providerElement = doc.GetElementByTestId("apprenticeship-assessment");
            var providerBadge = providerElement.GetElementsByClassName("govuk-tag").SingleOrDefault();
            Assert.NotNull(providerBadge);
        }

        [Fact]
        public async Task Get_ApprenticeshipAssessmentNotCompletedDoesNotRenderBadge()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var appElement = doc.GetAllElementsByTestId("apprenticeship-assessment").First();
            var appBadge = appElement.GetElementsByClassName("govuk-tag").SingleOrDefault();
            Assert.Null(appBadge);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_InProgressStatusAndSubmissionOutcomeIsKnownRendersFinishButton(bool passed)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
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

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var completeButton = doc.GetElementByTestId("complete-button");
            Assert.NotNull(completeButton);
            Assert.Null(completeButton.GetAttribute("disabled"));
        }

        [Fact]
        public async Task Get_InProgressStatusAndSubmissionOutcomeIsNotKnownRendersDisabledFinishButton()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var completeButton = doc.GetElementByTestId("complete-button");
            Assert.NotNull(completeButton);
            Assert.Equal("disabled", completeButton.GetAttribute("disabled"));
        }

        [Fact]
        public async Task Get_FailedStatusRendersText()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Failed);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: false,
                passed: false,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var completeButton = doc.GetElementByTestId("complete-button");
            Assert.Null(completeButton);
            var infoBox = doc.GetElementByTestId("status-text");
            Assert.NotNull(infoBox);
            Assert.Equal(
                "Overall the provider information and apprenticeship training course has FAILED quality assurance.",
                infoBox.TextContent.Trim());
        }

        [Fact]
        public async Task Get_PassedStatusRendersText()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: true,
                passed: true,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var completeButton = doc.GetElementByTestId("complete-button");
            Assert.Null(completeButton);
            var infoBox = doc.GetElementByTestId("status-text");
            Assert.NotNull(infoBox);
            Assert.Equal(
                "Overall the provider information and apprenticeship training course has PASSED quality assurance.",
                infoBox.TextContent.Trim());
        }

        [Fact]
        public async Task Get_NoSubmissionReturnsOk()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            await TestData.CreateUser(providerId: provider.ProviderId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
