using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class ProviderApprenticeshipQAInfoPanelTests : MvcTestBase
    {
        public ProviderApprenticeshipQAInfoPanelTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress,
                contacts: new[]
                {
                    new CreateProviderContact()
                    {
                        ContactType = "P",
                        AddressSaonDescription = "1st Line of Address",
                        AddressPaonDescription = "2nd Line of Address",
                        AddressStreetDescription = "The Street",
                        AddressLocality = "The Town",
                        AddressItems = new List<string>()
                        {
                            "United Kingdom"
                        },
                        AddressPostCode = "AB1 2CD",
                        ContactEmail = "email@provider1.com",
                        ContactTelephone1 = "01234 567890",
                        ContactWebsiteAddress = "provider1.com",
                        PersonalDetailsGivenName = "The",
                        PersonalDetailsFamilyName = "Contact"
                    }
                });

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUser.UserId, providerUserSignInDate);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: new DateTime(2018, 4, 12, 12, 0, 0, DateTimeKind.Utc),
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var assessorUser = await TestData.CreateUser();

            var lastAssessedOnDate = new DateTime(2018, 4, 12, 12, 3, 0, DateTimeKind.Utc);
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: assessorUser.UserId,
                lastAssessedOn: lastAssessedOnDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerapprenticeshipqainfopaneltests/{provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Equal(
                "Provider 1",
                doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-name").TextContent.Trim());
            Assert.Equal(
                provider.Ukprn.ToString(),
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-ukprn"));
            Assert.Equal(
                "1st Line of Address\n2nd Line of Address\nThe Street\nThe Town\nUnited Kingdom\nAB1 2CD",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-address"));
            Assert.Equal(
                "http://provider1.com",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-website"));
            Assert.Equal(
                "The Contact",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-contact-name"));
            Assert.Equal(
                "email@provider1.com",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-email"));
            Assert.Equal(
                "01234 567890",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-telephone"));
            Assert.Equal(
                providerUser.FullName,
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-signed-in"));
            Assert.Equal(
                providerUserSignInDate.ToString("dd MMM yyyy"),
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-signed-in-date"));
            Assert.Equal(
                assessorUser.FullName,
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-qa-by"));
            Assert.Equal(
                lastAssessedOnDate.ToString("dd MMM yyyy"),
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-qa-date"));
        }

        [Fact]
        public async Task NotAssessedYetUsesDoesNotRenderPanel()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;

            await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.CreateUserSignIn(providerUser.UserId, new DateTime(2018, 4, 12, 11, 30, 10, DateTimeKind.Utc));

            await User.AsHelpdesk();

            Clock.UtcNow = new DateTime(2018, 5, 3, 9, 3, 27, DateTimeKind.Utc);

            // Act
            var response = await HttpClient.GetAsync($"providerapprenticeshipqainfopaneltests/{provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-last-qa-by"));
            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-last-qa-date"));
        }

        [Fact]
        public async Task NoPTypeContactRendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress,
                contacts: new[]
                {
                    new CreateProviderContact()
                    {
                        ContactType = "L",
                        AddressSaonDescription = "1st Line of Address",
                        AddressPaonDescription = "2nd Line of Address",
                        AddressStreetDescription = "The Street",
                        AddressLocality = "The Town",
                        AddressItems = new List<string>()
                        {
                            "United Kingdom"
                        },
                        AddressPostCode = "AB1 2CD",
                        ContactEmail = "email@provider1.com",
                        ContactTelephone1 = "01234 567890",
                        ContactWebsiteAddress = "provider1.com",
                        PersonalDetailsFamilyName = "The",
                        PersonalDetailsGivenName = "Contact"
                    }
                });

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = (await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo())).ApprenticeshipId;

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUser.UserId, providerUserSignInDate);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                provider.ProviderId,
                submittedOn: new DateTime(2018, 4, 12, 12, 0, 0, DateTimeKind.Utc),
                submittedByUserId: providerUser.UserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var assessorUser = await TestData.CreateUser();

            var lastAssessedOnDate = new DateTime(2018, 4, 12, 12, 3, 0, DateTimeKind.Utc);
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: assessorUser.UserId,
                lastAssessedOn: lastAssessedOnDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerapprenticeshipqainfopaneltests/{provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-address"));
            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-website"));
            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-contact-name"));
            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-email"));
            Assert.Null(doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-telephone"));
        }

        private string GetBlockLabelContent(IHtmlDocument doc, string id)
        {
            var block = doc.GetElementById(id);
            return block.GetElementsByTagName("div").Single().TextContent.Trim();
        }
    }

    public class ProviderApprenticeshipQAInfoPanelTestsController : Controller
    {
        [HttpGet("providerapprenticeshipqainfopaneltests/{providerId}")]
        public IActionResult Get(Guid providerId) =>
            View("~/FeatureTests/ApprenticeshipQA/ProviderApprenticeshipQAInfoPanel.cshtml", providerId);
    }
}
