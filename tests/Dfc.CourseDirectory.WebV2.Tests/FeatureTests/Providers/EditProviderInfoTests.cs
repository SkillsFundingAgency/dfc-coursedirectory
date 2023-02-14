using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Providers
{
    public class EditProviderInfoTests : MvcTestBase
    {
        public EditProviderInfoTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUser_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.Apprenticeships,
                marketingInformation: "Current overview");

            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ProviderType.FE)]
        public async Task Get_NotApprenticeshipProvider_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: providerType,
                marketingInformation: "Current overview");

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"/providers/info?providerId={provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(ProviderType.FE)]
        public async Task Post_NotApprenticeshipProvider_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: providerType);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Overview")
                .ToContent();

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={provider.ProviderId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_ProviderUser_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Overview")
                .ToContent();

            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={provider.ProviderId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidMarketingInformation_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", new string('z', 751))  // Limit is 750 characters
                .ToContent();

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={provider.ProviderId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "MarketingInformation",
                "Brief overview of your organisation for employers must be 750 characters or fewer");
        }

        [Fact]
        public async Task Post_ValidRequest_ReturnsRedirect()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("MarketingInformation", "Overview")
                .ToContent();

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.PostAsync($"/providers/info?providerId={provider.ProviderId}", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/providers?providerId={provider.ProviderId}", response.Headers.Location.OriginalString);
        }
    }
}
