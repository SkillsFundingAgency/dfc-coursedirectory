using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class ProviderDetailInfoPanelTests : MvcTestBase
    {
        public ProviderDetailInfoPanelTests(CourseDirectoryApplicationFactory factory)
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

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUserId, providerUserSignInDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerdetailsinfopaneltests/{providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Equal(
                "1st Line of Address\n2nd Line of Address\nThe Street\nThe Town\nUnited Kingdom\nAB1 2CD",
                GetBlockLabelContent(doc, "pttcd-new-apprenticeship-provider__provider-detail-info-panel__provider-address"));
            Assert.Equal(
                "http://provider1.com",
                GetBlockLabelContent(doc, "pttcd-new-apprenticeship-provider__provider-detail-info-panel__provider-website"));
            Assert.Equal(
                "The Contact",
                GetBlockLabelContent(doc, "pttcd-new-apprenticeship-provider__provider-detail-info-panel__provider-contact-name"));
            Assert.Equal(
                "email@provider1.com",
                GetBlockLabelContent(doc, "pttcd-new-apprenticeship-provider__provider-detail-info-panel__provider-email"));
            Assert.Equal(
                "01234 567890",
                GetBlockLabelContent(doc, "pttcd-new-apprenticeship-provider__provider-detail-info-panel__provider-telephone"));
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

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUserId, providerUserSignInDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerdetailsinfopaneltests/{providerId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Null(doc.GetElementById("pttcd-detail-info-panel__provider-address"));
            Assert.Null(doc.GetElementById("pttcd-detail-info-panel__provider-website"));
            Assert.Null(doc.GetElementById("pttcd-detail-info-panel__provider-contact-name"));
            Assert.Null(doc.GetElementById("pttcd-detail-info-panel__provider-email"));
            Assert.Null(doc.GetElementById("pttcd-detail-info-panel__provider-telephone"));
        }

        private string GetBlockLabelContent(IHtmlDocument doc, string id)
        {
            var block = doc.GetElementById(id);
            return block.GetElementsByTagName("div").Single().TextContent.Trim();
        }
    }

    public class ProviderDetailsInfoPanelTestsController : Controller
    {
        [HttpGet("providerdetailsinfopaneltests/{providerId}")]
        public IActionResult Get(Guid providerId) =>
            View("~/FeatureTests/NewApprenticeshipProvider/ProviderDetailInfoPanel.cshtml", providerId);
    }
}
