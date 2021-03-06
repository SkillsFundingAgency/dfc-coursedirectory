﻿using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class StatusTests : MvcTestBase
    {
        public StatusTests(CourseDirectoryApplicationFactory factory)
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
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateApprenticeshipQAUnableToCompleteInfo(
                provider.ProviderId,
                ApprenticeshipQAUnableToCompleteReasons.ProviderHasAppliedToTheWrongRoute | ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision,
                comments: "Some feedback",
                addedByUserId: User.UserId,
                addedOn: Clock.UtcNow);

            await User.AsTestUser(testUserType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}/status");

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
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{providerId}/status");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        public async Task Get_InvalidStatusReturnsBadRequest(ApprenticeshipQAStatus status)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: status);

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
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}/status");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_StatusIsNotUnableToCompleteRendersExpectedOutput()
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
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}/status");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("checked", doc.GetElementById("UnableToComplete-false").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToComplete-true").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-1").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-2").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-4").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-8").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-16").GetAttribute("checked"));
            Assert.Empty(doc.GetElementById("Comments").TextContent);
        }

        [Fact]
        public async Task Get_StatusIsUnableToCompleteRendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateApprenticeshipQAUnableToCompleteInfo(
                provider.ProviderId,
                ApprenticeshipQAUnableToCompleteReasons.ProviderHasAppliedToTheWrongRoute | ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision,
                comments: "Some feedback",
                addedByUserId: User.UserId,
                addedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/{provider.ProviderId}/status");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("UnableToComplete-false").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementById("UnableToComplete-true").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-1").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementById("UnableToCompleteReasons-2").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-4").GetAttribute("checked"));
            Assert.Equal("checked", doc.GetElementById("UnableToCompleteReasons-8").GetAttribute("checked"));
            Assert.Null(doc.GetElementById("UnableToCompleteReasons-16").GetAttribute("checked"));
            Assert.Equal("Some feedback", doc.GetElementById("Comments").TextContent);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessReturnsForbidden(TestUserType testUserType)
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_ProviderDoesNotExistReturnsBadRequest()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.FalseString)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{providerId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(ApprenticeshipQAStatus.Passed)]
        public async Task Post_InvalidStatusReturnsBadRequest(ApprenticeshipQAStatus status)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: status);

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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.FalseString)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_UnableToCompleteMissingReasonsReturnsBadRequest()
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("UnableToCompleteReasons", "A reason must be selected");
        }

        [Fact]
        public async Task Post_UnableToCompleteStandardNotReadyMissingStandardNameReturnsBadRequest()
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .Add("UnableToCompleteReasons", "1")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("StandardName", "Enter the standard name");
        }

        [Fact]
        public async Task Post_UnableToCompleteOtherMissingCommentsReturnsBadRequest()
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .Add("UnableToCompleteReasons", "16")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Comments", "Enter comments for the reason selected");
        }

        [Fact]
        public async Task Post_ValidRequestUnableToCompleteAddsToStatus()
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .Add("UnableToCompleteReasons", "2")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            var newStatus = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = provider.ProviderId
                }));
            Assert.Equal(ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete, newStatus);
        }

        [Fact]
        public async Task Post_ValidRequestUnableToCompleteStandardNotReadyReturnsBadRequest()
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .Add("UnableToCompleteReasons", "2")
                .Add("Standard Name", "The standard")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Post_ValidRequestUnableToCompleteOtherReturnsBadRequest()
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

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.TrueString)
                .Add("UnableToCompleteReasons", "16")
                .Add("Comments", "Comments")
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Post_ValidRequestNotUnableToCompleteResetToPreviousState()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted | ApprenticeshipQAStatus.UnableToComplete);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).Id;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateApprenticeshipQAUnableToCompleteInfo(
                provider.ProviderId,
                ApprenticeshipQAUnableToCompleteReasons.ProviderHasAppliedToTheWrongRoute | ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision,
                comments: "Some feedback",
                addedByUserId: User.UserId,
                addedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("UnableToComplete", bool.FalseString)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync($"apprenticeship-qa/{provider.ProviderId}/status", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            var newStatus = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = provider.ProviderId
                }));
            Assert.Equal(ApprenticeshipQAStatus.Submitted, newStatus);
        }
    }
}
