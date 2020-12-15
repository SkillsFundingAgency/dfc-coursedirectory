using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class AuthorizeTLevelAttributeTests : MvcTestBase
    {
        public AuthorizeTLevelAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUsers_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeTLevelAttributeTests/{tLevel.TLevelId}");

            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsersForSameProviderAsCourse_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeTLevelAttributeTests/{tLevel.TLevelId}");

            await User.AsTestUser(userType, providerId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsersForDifferentProviderAsCourse_AreBlocked(TestUserType userType)
        {
            // Arrange
            var anotherProviderId = await TestData.CreateProvider(ukprn: 23456);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeTLevelAttributeTests/{tLevel.TLevelId}");

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnauthenticatedUser_IsBlocked()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var providerUser = await TestData.CreateUser("provider-user", "user@provider.com", "Test", "User", providerId);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeTLevelAttributeTests/{tLevel.TLevelId}");

            User.SetNotAuthenticated();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Route("AuthorizeTLevelAttributeTests/{tLevelId}")]
    public class AuthorizeTLevelAttributeTestsController : Controller
    {
        [HttpGet("")]
        [AllowAnonymous]  // Disable the up-front authentication check so our behavior gets executed
        [AuthorizeTLevel]
        public IActionResult Get() => Ok();
    }
}
