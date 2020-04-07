using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.WebV2.Models;
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
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
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

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUserId, providerUserSignInDate);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: new DateTime(2018, 4, 12, 12, 0, 0, DateTimeKind.Utc),
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var assessorUserId = "assessor";
            await TestData.CreateUser(assessorUserId, "assessor@place.com", "Mr", "Assessor", providerId: null);

            var lastAssessedOnDate = new DateTime(2018, 4, 12, 12, 3, 0, DateTimeKind.Utc);
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: assessorUserId,
                lastAssessedOn: lastAssessedOnDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerapprenticeshipqainfopaneltests/{providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Equal(
                "Provider 1",
                doc.GetElementById("pttcd-apprenticeship-qa-info-panel__provider-name").TextContent.Trim());
            Assert.Equal(
                "12345",
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
                "Provider 1 Person",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-signed-in"));
            Assert.Equal(
                providerUserSignInDate.ToString("dd MMM yyyy"),
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-signed-in-date"));
            Assert.Equal(
                "Mr Assessor",
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-qa-by"));
            Assert.Equal(
                lastAssessedOnDate.ToString("dd MMM yyyy"),
                GetBlockLabelContent(doc, "pttcd-apprenticeship-qa-info-panel__provider-last-qa-date"));
        }

        [Fact]
        public async Task NotAssessedYetUsesDoesNotRenderPanel()
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

            await TestData.CreateUserSignIn(providerUserId, new DateTime(2018, 4, 12, 11, 30, 10, DateTimeKind.Utc));

            await User.AsHelpdesk();

            Clock.UtcNow = new DateTime(2018, 5, 3, 9, 3, 27, DateTimeKind.Utc);

            // Act
            var response = await HttpClient.GetAsync($"providerapprenticeshipqainfopaneltests/{providerId}");

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
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
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

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId, standard);

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUserId, providerUserSignInDate);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: new DateTime(2018, 4, 12, 12, 0, 0, DateTimeKind.Utc),
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var assessorUserId = "assessor";
            await TestData.CreateUser(assessorUserId, "assessor@place.com", "Mr", "Assessor", providerId: null);

            var lastAssessedOnDate = new DateTime(2018, 4, 12, 12, 3, 0, DateTimeKind.Utc);
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: assessorUserId,
                lastAssessedOn: lastAssessedOnDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerapprenticeshipqainfopaneltests/{providerId}");

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
