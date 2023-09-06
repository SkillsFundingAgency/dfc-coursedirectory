using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ViewComponentTests
{
    public class ProviderInfoPanelTests : MvcTestBase
    {
        public ProviderInfoPanelTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                contacts: new[]
                {
                    new ProviderContact()
                    {
                        ContactType = "P",
                        AddressSaonDescription = "1st Line of Address",
                        AddressPaonDescription = "2nd Line of Address",
                        AddressStreetDescription = "The Street",
                        AddressLocality = "The Town",
                        AddressItems = "United Kingdom",
                        AddressPostcode = "AB1 2CD",
                        Email = "email@provider1.com",
                        Telephone1 = "01234 567890",
                        WebsiteAddress = "provider1.com",
                        PersonalDetailsPersonNameGivenName = "The",
                        PersonalDetailsPersonNameFamilyName = "Contact"
                    }
                });

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUser.UserId, providerUserSignInDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerinfopaneltests/{provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Equal(
                "1st Line of Address\n2nd Line of Address\nThe Street\nThe Town\nUnited Kingdom\nAB1 2CD",
                GetBlockLabelContent(doc, "ProviderInfoPanel-Address"));
            Assert.Equal(
                "http://provider1.com",
                GetBlockLabelContent(doc, "ProviderInfoPanel-Website"));
            Assert.Equal(
                "The Contact",
                GetBlockLabelContent(doc, "ProviderInfoPanel-ContactName"));
            Assert.Equal(
                "email@provider1.com",
                GetBlockLabelContent(doc, "ProviderInfoPanel-Email"));
            Assert.Equal(
                "01234 567890",
                GetBlockLabelContent(doc, "ProviderInfoPanel-Telephone"));
        }

        [Fact]
        public async Task NoPTypeContactRendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Provider 1",
                contacts: new[]
                {
                    new ProviderContact()
                    {
                        ContactType = "P",
                        AddressSaonDescription = "1st Line of Address",
                        AddressPaonDescription = "2nd Line of Address",
                        AddressStreetDescription = "The Street",
                        AddressLocality = "The Town",
                        AddressItems = "United Kingdom",
                        AddressPostcode = "AB1 2CD",
                        Email = "email@provider1.com",
                        Telephone1 = "01234 567890",
                        WebsiteAddress = "provider1.com",
                        PersonalDetailsPersonNameGivenName = "The",
                        PersonalDetailsPersonNameFamilyName = "Contact"
                    }
                });

            var providerUser = await TestData.CreateUser(providerId: provider.ProviderId);

            var providerUserSignInDate = new DateTime(2018, 4, 12, 11, 30, 0, DateTimeKind.Utc);
            await TestData.CreateUserSignIn(providerUser.UserId, providerUserSignInDate);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"providerinfopaneltests/{provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Null(doc.GetElementById("ProviderInfoPanel-Address"));
            Assert.Null(doc.GetElementById("ProviderInfoPanel-Website"));
            Assert.Null(doc.GetElementById("ProviderInfoPanel-ContactName"));
            Assert.Null(doc.GetElementById("ProviderInfoPanel-Email"));
            Assert.Null(doc.GetElementById("ProviderInfoPanel-Telephone"));
        }

        private string GetBlockLabelContent(IHtmlDocument doc, string testid)
        {
            var block = doc.GetElementByTestId(testid);
            return block.GetElementsByTagName("div").Single().TextContent.Trim();
        }
    }

    public class ProviderInfoPanelTestsController : Controller
    {
        [HttpGet("providerinfopaneltests/{providerId}")]
        public IActionResult Get(Guid providerId) =>
            View("~/ViewComponentTests/ProviderInfoPanel.cshtml", providerId);
    }
}
