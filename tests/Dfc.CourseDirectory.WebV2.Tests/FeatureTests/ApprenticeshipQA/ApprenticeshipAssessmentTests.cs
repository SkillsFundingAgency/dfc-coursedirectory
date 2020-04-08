using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ApprenticeshipAssessment;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
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
        public async Task GetStart_ProviderUser_ReturnsForbidden(TestUserType userType)
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
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessment/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetStart_ProviderDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            await User.AsHelpdesk();

            var providerId = Guid.NewGuid();

            var apprenticeshipId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessment/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetStart_ApprenticeshipDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            await User.AsHelpdesk();

            var providerId = await TestData.CreateProvider();

            var apprenticeshipId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/{providerId}/apprenticeship-assessment/{apprenticeshipId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetStart_NoSubmission_ReturnsBadRequest()
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
                $"apprenticeship-qa/{providerId}/apprenticeship-assessment/{apprenticeshipId}");

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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}");

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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Test Standard", doc.QuerySelector("h1").TextContent);
            Assert.Equal(
                "Marketing info",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-assessment-marketing-information").TextContent);
            AssertFormFieldsDisabledState(doc, expectDisabled: false);
            Assert.Empty(doc.GetElementsByClassName("govuk-back-link"));
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateApprenticeshipQAApprenticeshipAssessment(
                submissionId,
                apprenticeshipId: apprenticeshipId,
                assessedByUserId: User.UserId,
                assessedOn: Clock.UtcNow,
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded | ApprenticeshipQAApprenticeshipStyleFailedReasons.TermCourseUsed,
                styleComments: "Bad style, yo");

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}");

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

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            AssertFormFieldsDisabledState(doc, expectDisabled: true);
            Assert.NotEmpty(doc.GetElementsByClassName("govuk-back-link"));
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsTestUser(userType, providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAApprenticeshipComplianceFailedReasons.Other)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.Other)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                "/apprenticeship-qa/apprenticeship-assessment-confirmation",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", true)
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.TermFrameworkUsed)
                .Add("StyleComments", "Some feedback")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                "/apprenticeship-qa/apprenticeship-assessment-confirmation",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAApprenticeshipComplianceFailedReasons.IncorrectOfsetGradeUsed)
                .Add("ComplianceFailedReasons", ApprenticeshipQAApprenticeshipComplianceFailedReasons.UnverifiableClaim)
                .Add("ComplianceComments", "Some feedback")
                .Add("StylePassed", true)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                "/apprenticeship-qa/apprenticeship-assessment-confirmation",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("CompliancePassed", false)
                .Add("ComplianceFailedReasons", ApprenticeshipQAApprenticeshipComplianceFailedReasons.IncorrectOfsetGradeUsed)
                .Add("ComplianceFailedReasons", ApprenticeshipQAApprenticeshipComplianceFailedReasons.UnverifiableClaim)
                .Add("ComplianceComments", "Some compliance feedback")
                .Add("StylePassed", false)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded)
                .Add("StyleFailedReasons", ApprenticeshipQAApprenticeshipStyleFailedReasons.TermFrameworkUsed)
                .Add("StyleComments", "Some style feedback")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(
                "/apprenticeship-qa/apprenticeship-assessment-confirmation",
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

            var mptxInstance = MptxManager.CreateInstance("ApprenticeshipAssessment", new FlowModel()
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
                $"apprenticeship-qa/apprenticeship-assessment/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            Assert.False(mptxInstance.State.GotAssessmentOutcome);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}");

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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.None,
                styleComments: null));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "This apprenticeship training course has PASSED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded,
                styleComments: "Feedback"));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "This apprenticeship training course has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: false,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.UnverifiableClaim,
                complianceComments: "Feedback",
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.None,
                styleComments: null));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Pass", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "This apprenticeship training course has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: false,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.UnverifiableClaim,
                complianceComments: "Feedback",
                stylePassed: false,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded,
                styleComments: "Feedback"));

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("QA Apprenticeship training course information - Course Directory", doc.Title);
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Compliance"));
            Assert.Equal("Fail", doc.GetSummaryListValueWithKey("Style"));
            Assert.Equal(
                "This apprenticeship training course has FAILED quality assurance.",
                doc.GetElementById("pttcd-apprenticeship-qa-apprenticeship-submission-confirmation-overall-status-message").TextContent.Trim());
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.None,
                styleComments: null));

            await User.AsTestUser(userType, providerId);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}",
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

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: true,
                complianceFailedReasons: ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                complianceComments: null,
                stylePassed: true,
                styleFailedReasons: ApprenticeshipQAApprenticeshipStyleFailedReasons.None,
                styleComments: null));

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}",
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
            bool? providerAssessmentsPassed,
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
                providerAssessmentPassed: providerAssessmentsPassed,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId,
                lastAssessedOn: Clock.UtcNow);

            var mptxInstance = await CreateMptxInstance(apprenticeshipId);
            mptxInstance.Update(s => s.SetAssessmentOutcome(
                compliancePassed: compliancePassed,
                complianceFailedReasons: compliancePassed ?
                    ApprenticeshipQAApprenticeshipComplianceFailedReasons.None :
                    ApprenticeshipQAApprenticeshipComplianceFailedReasons.UnverifiableClaim,
                complianceComments: compliancePassed ? null : "Feedback",
                stylePassed: stylePassed,
                styleFailedReasons: stylePassed ?
                    ApprenticeshipQAApprenticeshipStyleFailedReasons.None :
                    ApprenticeshipQAApprenticeshipStyleFailedReasons.JobRolesIncluded,
                styleComments: stylePassed ? null : "Feedback"));

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeship-qa/apprenticeship-assessment-confirmation/?ffiid={mptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/apprenticeship-qa/{providerId}", response.Headers.Location.OriginalString);

            var submissionStatus = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                }));
            Assert.Equal(expectedSubmissionPassed, submissionStatus.AsT1.Passed);
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

        private async Task<MptxInstanceContext<FlowModel>> CreateMptxInstance(Guid apprenticeshipId)
        {
            var state = await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = CreateInstance<FlowModelInitializer>(dispatcher);
                return await initializer.Initialize(apprenticeshipId);
            });

            return CreateMptxInstance<FlowModel>("ApprenticeshipAssessment", state);
        }
    }
}
