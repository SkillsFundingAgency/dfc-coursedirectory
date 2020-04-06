using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class ApprenticeshipAssessmentTests : MvcTestBase
    {
        public ApprenticeshipAssessmentTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUserCannotAccess(TestUserType userType)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_ApprenticeshipDoesNotExistReturnsBadRequest()
        {
            // Arrange
            await User.AsHelpdesk();

            var providerId = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var apprenticeshipId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NoSubmissionReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NewSubmissionSucceeds()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(
                providerId,
                standard,
                marketingInformation: "Test marketing info");

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Test Standard", doc.QuerySelector("h1").TextContent);
            Assert.Equal(
                "Test marketing info",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-assessment-marketing-information").TextContent);
            AssertFormFieldsDisabledState(doc, expectDisabled: false);
            Assert.Empty(doc.GetElementsByClassName("govuk-back-link"));
        }

        [Fact]
        public async Task Get_AlreadyAssessedSubmissionRendersExpectedOutput()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(
                providerId,
                standard,
                marketingInformation: "Test marketing info");

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateApprenticeshipQAApprenticeshipAssessment(
                submissionId,
                apprenticeshipId,
                assessedByUserId: User.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded | ApprenticeshipQAApprenticeshipStyleFailedReasons.TermCourseUsed,
                styleComments: "Bad style, yo");

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("checked", doc.GetElementById("CompliancePassed").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("StylePassed").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementWithLabel("Job roles included").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementWithLabel("Term 'course' used").GetAttribute("checked"));
            Assert.Equal("Bad style, yo", doc.GetElementById("StyleComments").TextContent);
        }

        [Theory]
        [InlineData(true, ApprenticeshipQAStatus.Passed)]
        [InlineData(false, ApprenticeshipQAStatus.Failed)]
        [InlineData(false, ApprenticeshipQAStatus.UnableToComplete | ApprenticeshipQAStatus.NotStarted)]
        public async Task Get_QAStatusNotValidRendersReadOnly(bool passed, ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: qaStatus);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(
                providerId,
                standard,
                marketingInformation: "Test marketing info");

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

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            AssertFormFieldsDisabledState(doc, expectDisabled: true);
            Assert.NotEmpty(doc.GetElementsByClassName("govuk-back-link"));
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_ProviderUserCannotAccess(TestUserType userType)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsTestUser(userType, providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                 .Add("CompliancePassed", true)
                 .Add("StylePassed", true)
                 .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_ApprenticeshipDoesNotExistReturnsBadRequest()
        {
            // Arrange
            await User.AsHelpdesk();

            var providerId = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var apprenticeshipId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                 .Add("CompliancePassed", true)
                 .Add("StylePassed", true)
                 .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_NoSubmissionReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                 .Add("CompliancePassed", true)
                 .Add("StylePassed", true)
                 .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete | ApprenticeshipQAStatus.NotStarted)]
        public async Task Post_SubmissionAtInvalidStatusReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: qaStatus);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();
            
            var requestContent = new FormUrlEncodedContentBuilder()
                 .Add("CompliancePassed", true)
                 .Add("StylePassed", true)
                 .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_MissingCompliancePassedRendersErrorMessage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("CompliancePassed", "An outcome must be selected");
        }
        
        [Fact]
        public async Task Post_MissingComplianceFailedReasonsWhenFailedRendersErrorMessage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("ComplianceFailedReasons", "A reason must be selected");
        }

        [Fact]
        public async Task Post_MissingComplianceCommentsWhenReasonContainsOtherRendersErrorMessage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAApprenticeshipComplianceFailedReasons.Other)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("ComplianceComments", "Enter comments for the reason selected");
        }

        [Fact]
        public async Task Post_MissingStylePassedRendersErrorMessage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StylePassed", "An outcome must be selected");
        }

        [Fact]
        public async Task Post_MissingStyleFailedReasonsWhenFailedRendersErrorMessage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StyleFailedReasons", "A reason must be selected");
        }

        [Fact]
        public async Task Post_MissingStyleCommentsWhenReasonContainsOtherRendersErrorMessage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.Other)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StyleComments", "Enter comments for the reason selected");
        }

        [Fact]
        public async Task Post_ComplianceAndStylePassedUpdatesSubmissionStatusAndRendersConfirmationPage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Pass", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-compliance-status").TextContent.Trim());
            Assert.Equal("Pass", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-style-status").TextContent.Trim());
            Assert.Equal(
                "This apprenticeship training course has PASSED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.True(submissionStatus.AsT1.ApprenticeshipAssessmentsPassed.Value);
        }

        [Fact]
        public async Task Post_CompliancePassedAndStyleFailedUpdatesSubmissionStatusAndRendersConfirmationPage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded)
                .Add("StyleFailedReasons", ApprenticeshipQAProviderStyleFailedReasons.TermFrameworkUsed)
                .Add("StyleComments", "Some feedback")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Pass", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-compliance-status").TextContent.Trim());
            Assert.Equal("Fail", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-style-status").TextContent.Trim());
            Assert.Equal(
                "This apprenticeship training course has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.False(submissionStatus.AsT1.ApprenticeshipAssessmentsPassed.Value);
        }

        [Fact]
        public async Task Post_ComplianceFailedAndStylePassedUpdatesSubmissionStatusAndRendersConfirmationPage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAProviderComplianceFailedReasons.IncorrectOfsetGradeUsed)
                .Add("ComplianceFailedReasons", ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim)
                .Add("ComplianceComments", "Some feedback")
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Fail", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-compliance-status").TextContent.Trim());
            Assert.Equal("Pass", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-style-status").TextContent.Trim());
            Assert.Equal(
                "This apprenticeship training course has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.False(submissionStatus.AsT1.ApprenticeshipAssessmentsPassed.Value);
        }

        [Fact]
        public async Task Post_ComplianceAndStyleFailedUpdatesSubmissionStatusAndRendersConfirmationPage()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAProviderComplianceFailedReasons.IncorrectOfsetGradeUsed)
                .Add("ComplianceFailedReasons", ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim)
                .Add("ComplianceComments", "Some compliance feedback")
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded)
                .Add("StyleFailedReasons", ApprenticeshipQAProviderStyleFailedReasons.TermFrameworkUsed)
                .Add("StyleComments", "Some style feedback")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Fail", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-compliance-status").TextContent.Trim());
            Assert.Equal("Fail", doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-style-status").TextContent.Trim());
            Assert.Equal(
                "This apprenticeship training course has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.False(submissionStatus.AsT1.ApprenticeshipAssessmentsPassed.Value);
        }

        [Theory]
        [InlineData(true, true, null, null)]
        [InlineData(true, false, null, null)]
        [InlineData(false, true, null, null)]
        [InlineData(false, false, null, null)]
        [InlineData(true, true, true, true)]
        [InlineData(true, false, true, false)]
        [InlineData(false, true, true, false)]
        [InlineData(false, false, true, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, false, false)]
        public async Task Post_UpdatesSubmissionStatusCorrectly(
            bool compliancePassed,
            bool stylePassed,
            bool? providerAssessmentPassed,
            bool? expectedSubmissionPassed)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: providerAssessmentPassed,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            var requestContentBuilder = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", compliancePassed)
                .Add("ComplianceComments", "Some compliance feedback")
                .Add("StylePassed", stylePassed);

            if (!compliancePassed)
            {
                requestContentBuilder
                    .Add("ComplianceFailedReasons", (ApprenticeshipQAApprenticeshipComplianceFailedReasons)1)
                    .Add("ComplianceComments", "Compliance feedback");
            }

            if (!stylePassed)
            {
                requestContentBuilder
                    .Add("StyleFailedReasons", (ApprenticeshipQAApprenticeshipStyleFailedReasons)1)
                    .Add("StyleComments", "Style feedback");
            }

            var requestContent = requestContentBuilder.ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.Equal(expectedSubmissionPassed, submissionStatus.AsT1.Passed);
        }

        [Theory]
        [InlineData(true, ApprenticeshipQAStatus.Passed)]
        [InlineData(false, ApprenticeshipQAStatus.Failed)]
        [InlineData(false, ApprenticeshipQAStatus.UnableToComplete | ApprenticeshipQAStatus.NotStarted)]
        public async Task Post_QAStatusNotValidReturnsBadRequest(bool passed, ApprenticeshipQAStatus qaStatus)
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: qaStatus);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessments/{apprenticeshipId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private void AssertFormFieldsDisabledState(IHtmlDocument doc, bool expectDisabled)
        {
            var fields = doc.GetElementsByTagName("input")
                .Concat(doc.GetElementsByTagName("textarea"));

            foreach (var f in fields)
            {
                if (f.GetAttribute("name") == "__RequestVerificationToken")
                {
                    continue;
                }

                if (expectDisabled)
                {
                    Assert.Equal("disabled", f.GetAttribute("disabled"));
                }
                else
                {
                    Assert.Null(f.GetAttribute("disabled"));
                }
            }
        }
    }
}
