using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderAssessment;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Xunit;
using Dfc.CourseDirectory.Testing;
using FormFlow;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class ProviderAssessmentTests : MvcTestBase
    {
        public ProviderAssessmentTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUser_ReturnsForbidden(TestUserType userType)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/provider-assessment/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_ProviderDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            await User.AsHelpdesk();

            var providerId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/provider-assessment/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NoSubmission_ReturnsBadRequest()
        {
            // Arrange
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/provider-assessment/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        public async Task Get_InvalidQAStatus_ReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_NewSubmission_RendersExpectedOutput()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Provider 1", doc.QuerySelector("h1").TextContent);
            Assert.Equal(
                "The overview",
                doc.GetElementById("pttcd-apprenticeship-qa-provider-assessment-marketing-information").TextContent);
            AssertFormFieldsDisabledState(doc, expectDisabled: false);
            Assert.Null(doc.GetElementByTestId("back"));
        }

        [Fact]
        public async Task Get_AlreadyAssessedSubmission_RendersExpectedOutput()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                assessedByUserId: User.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded | ApprenticeshipQAProviderStyleFailedReasons.TermCourseUsed,
                styleComments: "Bad style, yo");

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}");

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
        public async Task Get_CannotCreateSubmission_RendersReadOnly(bool passed, ApprenticeshipQAStatus qaStatus)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

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

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            AssertFormFieldsDisabledState(doc, expectDisabled: true);
            Assert.NotNull(doc.GetElementByTestId("back"));
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_ProviderUser_ReturnsForbidden(TestUserType userType)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsTestUser(userType, providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete | ApprenticeshipQAStatus.NotStarted)]
        public async Task Post_InvalidQAStatus_ReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();
            
            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_MissingCompliancePassed_RendersErrorMessage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("CompliancePassed", "An outcome must be selected");
        }

        [Fact]
        public async Task Post_MissingComplianceFailedReasonsWhenFailed_RendersErrorMessage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("ComplianceFailedReasons", "A reason must be selected");
        }

        [Fact]
        public async Task Post_MissingComplianceCommentsWhenReasonContainsOther_RendersErrorMessage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAProviderComplianceFailedReasons.Other)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("ComplianceComments", "Enter comments for the reason selected");
        }

        [Fact]
        public async Task Post_MissingStylePassed_RendersErrorMessage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StylePassed", "An outcome must be selected");
        }

        [Fact]
        public async Task Post_MissingStyleFailedReasonsWhenFailed_RendersErrorMessage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StyleFailedReasons", "A reason must be selected");
        }

        [Fact]
        public async Task Post_MissingStyleCommentsWhenReasonContainsOther_RendersErrorMessage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.Other)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StyleComments", "Enter comments for the reason selected");
        }

        [Fact]
        public async Task Post_CompliancePassedAndStylePassed_RedirectsToConfirmationPage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                $"/apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task Post_CompliancePassedAndStyleFailed_RedirectsToConfirmationPage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

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
                $"/apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                $"/apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task Post_ComplianceFailedAndStylePassed_RedirectsToConfirmationPage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

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
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                $"/apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task Post_ComplianceFailedAndStyleFailed_RedirectsToConfirmationPage()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await CreateFormFlowInstance(providerId);

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
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                $"/apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

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

            await CreateFormFlowInstance(
                providerId,
                new FlowModel()
                {
                    ProviderId = providerId
                });

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetConfirmation_NotGotOutcome_ReturnsBadRequest()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            Assert.False(formFlowInstance.State.GotAssessmentOutcome);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetConfirmation_PassedComplianceAndPassedStyle_RendersExpectedContent()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.None,
                styleComments: null));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "The provider information has PASSED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-provider-submission-confirmation-overall-status-message").TextContent.Trim());
        }

        [Fact]
        public async Task GetConfirmation_PassedComplianceAndFailedStyle_RendersExpectedContent()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded,
                styleComments: "Feedback"));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "The provider information has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-provider-submission-confirmation-overall-status-message").TextContent.Trim());
        }

        [Fact]
        public async Task GetConfirmation_FailedComplianceAndPassedStyle_RendersExpectedContent()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: false,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim,
                complianceComments: "Feedback",
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.None,
                styleComments: null));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "The provider information has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-provider-submission-confirmation-overall-status-message").TextContent.Trim());
        }

        [Fact]
        public async Task GetConfirmation_FailedComplianceAndFailedStyle_RendersExpectedContent()
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: false,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim,
                complianceComments: "Feedback",
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded,
                styleComments: "Feedback"));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "The provider information has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-provider-submission-confirmation-overall-status-message").TextContent.Trim());
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task PostConfirmation_ProviderUser_ReturnsForbidden(TestUserType userType)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.None,
                styleComments: null));

            await User.AsTestUser(userType, providerId);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Failed)]
        [InlineData(ApprenticeshipQAStatus.NotStarted)]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        [InlineData(ApprenticeshipQAStatus.UnableToComplete | ApprenticeshipQAStatus.NotStarted)]
        public async Task PostConfirmation_InvalidQAStatus_ReturnsBadRequest(ApprenticeshipQAStatus qaStatus)
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAProviderComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAProviderStyleFailedReasons.None,
                styleComments: null));

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
        public async Task PostConfirmation_UpdatesSubmissionStatusCorrectlyAndRedirects(
            bool compliancePassed,
            bool stylePassed,
            bool? apprenticeshipAssessmentsPassed,
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard, createdBy: User.ToUserInfo());

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: null,
                apprenticeshipAssessmentsPassed: apprenticeshipAssessmentsPassed,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            var formFlowInstance = await CreateFormFlowInstance(providerId);
            formFlowInstance.UpdateState(s => s.SetAssessmentOutcome(
                compliancePassed: compliancePassed,
                complianceFailedReasons: compliancePassed ?
                    ApprenticeshipQAProviderComplianceFailedReasons.None :
                    ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim,
                complianceComments: compliancePassed ? null : "Feedback",
                stylePassed: stylePassed,
                styleFailedReasons: stylePassed ?
                    ApprenticeshipQAProviderStyleFailedReasons.None :
                    ApprenticeshipQAProviderStyleFailedReasons.JobRolesIncluded,
                styleComments: stylePassed ? null : "Feedback"));

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeship-qa/provider-assessment/{providerId}/confirmation",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/apprenticeship-qa/{providerId}", response.Headers.Location.OriginalString);

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.Equal(expectedSubmissionPassed, submissionStatus.Passed);
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

        private async Task<FormFlowInstance<FlowModel>> CreateFormFlowInstance(
            Guid providerId,
            FlowModel state = null)
        {
            state ??= await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = CreateInstance<FlowModelInitializer>(dispatcher);
                return await initializer.Initialize(providerId);
            });

            return CreateFormFlowInstanceForRouteParameters(
                key: "apprenticeship-qa/provider-assessment",
                routeParameters: new Dictionary<string, object>()
                {
                    { "providerId", providerId }
                },
                state);
        }
    }
}
