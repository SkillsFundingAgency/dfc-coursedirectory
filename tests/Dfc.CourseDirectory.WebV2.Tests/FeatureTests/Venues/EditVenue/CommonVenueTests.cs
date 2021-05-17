using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.EditVenue
{
    public class CommonVenueTests: MvcTestBase
    {
        public CommonVenueTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData("", TestUserType.ProviderSuperUser)] // details
        [InlineData("", TestUserType.ProviderUser)] // details
        [InlineData("/email", TestUserType.ProviderSuperUser)]
        [InlineData("/email", TestUserType.ProviderUser)]
        [InlineData("/address", TestUserType.ProviderSuperUser)]
        [InlineData("/address", TestUserType.ProviderUser)]
        [InlineData("/name", TestUserType.ProviderSuperUser)]
        [InlineData("/name", TestUserType.ProviderUser)]
        [InlineData("/phone-number", TestUserType.ProviderSuperUser)]
        [InlineData("/phone-number", TestUserType.ProviderUser)]
        [InlineData("/website", TestUserType.ProviderSuperUser)]
        [InlineData("/website", TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessVenue_ReturnsForbidden(string urlFragment, TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            var anotherProvider = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}{urlFragment}");

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("")] // details
        [InlineData("/email")]
        [InlineData("/address")]
        [InlineData("/name")]
        [InlineData("/phone-number")]
        [InlineData("/website")]
        public async Task Get_VenueDoesNotExist_ReturnsNotFound(string urlFragment)
        {
            // Arrange
            var venueId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}{urlFragment}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
