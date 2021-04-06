using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class AuthorizeVenueAttributeTests : MvcTestBase
    {
        public AuthorizeVenueAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUsers_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeVenueAttributeTests/{venueId}");

            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsersForSameProviderAsVenue_AreNotBlocked(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeVenueAttributeTests/{venueId}");

            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUsersForDifferentProviderAsVenue_AreBlocked(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider();
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeVenueAttributeTests/{venueId}");

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnauthenticatedUser_IsBlocked()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/AuthorizeVenueAttributeTests/{venueId}");

            User.SetNotAuthenticated();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Route("AuthorizeVenueAttributeTests/{venueId}")]
    public class AuthorizeVenueAttributeTestsController : Controller
    {
        [HttpGet("")]
        [AllowAnonymous]  // Disable the up-front authentication to ensure filter gets executed
        [AuthorizeVenue]
        public IActionResult Get() => Ok();
    }
}
