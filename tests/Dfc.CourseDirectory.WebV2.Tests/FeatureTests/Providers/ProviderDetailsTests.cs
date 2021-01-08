using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers
{
    public class ProviderDetailsTests : MvcTestBase
    {
        public ProviderDetailsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ProviderDoesNotExist_ReturnsRedirect()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsDeveloper();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessProvider_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                ukprn: 12345,
                providerName: "My Provider",
                providerType: ProviderType.FE,
                providerStatus: "Active",
                alias: "My Trading Name");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ValidRequest_RendersExpectedContent(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                ukprn: 12345,
                providerName: "My Provider",
                providerType: ProviderType.FE,
                providerStatus: "Active",
                alias: "My Trading Name");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("My Provider", doc.GetElementByTestId("ProviderName").TextContent);
            Assert.Equal("Active", doc.GetSummaryListValueWithKey("Course directory status"));
            Assert.Equal("12345", doc.GetSummaryListValueWithKey("UKPRN"));
            Assert.Equal("My Trading Name", doc.GetSummaryListValueWithKey("Trading name"));
            Assert.Equal("FE Courses", doc.GetSummaryListValueWithKey("Provider type"));
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ValidRequestWithTLevelsProviderWithSelectedTLevelsDefinitions_RendersExpectedContent(TestUserType userType)
        {
            var providerId = await TestData.CreateProvider(providerType: ProviderType.TLevels);

            var tLevelDefinitionIds = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).Select(id => TestData.CreateTLevelDefinition(tLevelDefinitionId: id, name: $"Name-{id}")));
            var selectedTLevelDefinitionIds = tLevelDefinitionIds.OrderBy(_ => Guid.NewGuid()).Take(2).ToArray();
            await TestData.SetProviderTLevelDefinitions(providerId, selectedTLevelDefinitionIds);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            var providerTypeValue = doc.GetSummaryListValueWithKey("Provider type");

            doc.GetAllElementsByTestId("providerType").Select(e => e.TextContent.Trim()).Should().OnlyContain(p => p == "T Levels" );

            var tLevelNames = doc.GetAllElementsByTestId("tLevelName").Select(e => e.TextContent.Trim());

            foreach (var selectedTLevelDefinitionId in selectedTLevelDefinitionIds)
            {
                tLevelNames.Should().Contain($"Name-{selectedTLevelDefinitionId}");
            }

            foreach (var selectedTLevelDefinitionId in tLevelDefinitionIds.Except(selectedTLevelDefinitionIds))
            {
                tLevelNames.Should().NotContain($"Name-{selectedTLevelDefinitionId}");
            }
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task Get_UserIsAdmin_DoesRenderChangeProviderTypeLink(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                ukprn: 12345,
                providerName: "My Provider",
                providerType: ProviderType.FE,
                providerStatus: "Active",
                alias: "My Trading Name");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementByTestId("ChangeProviderType"));
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserIsNotAdmin_DoesNotRenderChangeProviderTypeLink(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                ukprn: 12345,
                providerName: "My Provider",
                providerType: ProviderType.FE,
                providerStatus: "Active",
                alias: "My Trading Name");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementByTestId("ChangeProviderType"));
        }

        [Theory]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        [InlineData(ProviderType.TLevels | ProviderType.Apprenticeships)]
        public async Task Get_ApprenticeshipProviderType_RendersMarketingInformation(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: providerType,
                marketingInformation: "Marketing information");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Marketing information", doc.GetSummaryListValueWithKey("Provider information"));
        }

        [Theory]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.TLevels)]
        public async Task Get_NotApprenticeshipProviderType_DoesNotRenderMarketingInformation(ProviderType providerType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: providerType,
                marketingInformation: "Marketing information");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementByTestId("MarketingInformation"));
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task Get_UserIsAdmin_DoesRenderChangeMarketingInformationLink(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                marketingInformation: "Marketing information");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementByTestId("ChangeMarketingInformation"));
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserIsNotAdmin_DoesNotRenderChangeMarketingInformationLink(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                marketingInformation: "Marketing information");

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementByTestId("ChangeMarketingInformation"));
        }

        [Theory]
        [InlineData(ProviderDisplayNameSource.ProviderName, "My Provider")]
        [InlineData(ProviderDisplayNameSource.TradingName, "My Trading Name")]
        public async Task Get_ProviderHasAlias_RendersCorrectDisplayName(
            ProviderDisplayNameSource displayNameSource,
            string expectedDisplayName)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerName: "My Provider",
                alias: "My Trading Name",
                displayNameSource: displayNameSource);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal(expectedDisplayName, doc.GetSummaryListValueWithKey("Display name"));
        }

        [Theory]
        [InlineData(ProviderDisplayNameSource.ProviderName)]
        [InlineData(ProviderDisplayNameSource.TradingName)]
        public async Task Get_ProviderHasAlias_RendersChangeDisplayNameLink(ProviderDisplayNameSource displayNameSource)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerName: "My Provider",
                alias: "My Trading Name",
                displayNameSource: displayNameSource);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementByTestId("ChangeDisplayName"));
        }

        [Theory]
        [InlineData(ProviderDisplayNameSource.ProviderName)]
        [InlineData(ProviderDisplayNameSource.TradingName)]
        public async Task Get_ProviderUserAndProviderHasAlias_DoesNotRenderChangeDisplayNameLink(
            ProviderDisplayNameSource displayNameSource)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.Apprenticeships,
                providerName: "My Provider",
                alias: "My Trading Name",
                displayNameSource: displayNameSource);

            var request = new HttpRequestMessage(HttpMethod.Get, $"providers?providerId={providerId}");

            await User.AsProviderUser(providerId, ProviderType.FE | ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementByTestId("ChangeDisplayName"));
        }
    }
}
