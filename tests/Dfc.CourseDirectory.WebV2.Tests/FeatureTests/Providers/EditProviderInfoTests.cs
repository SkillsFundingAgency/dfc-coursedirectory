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
    }
}
