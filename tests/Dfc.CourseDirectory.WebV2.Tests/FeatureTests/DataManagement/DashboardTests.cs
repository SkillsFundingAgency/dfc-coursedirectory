using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement
{
    public class DashboardTests : MvcTestBase
    {
        public DashboardTests(CourseDirectoryApplicationFactory factory) : base(factory)
        {
        }

        [Theory]
        [InlineData(ProviderType.None, false, false)]
        [InlineData(ProviderType.FE, true, false)]
        [InlineData(ProviderType.Apprenticeships, false, true)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships, true, true)]
        [InlineData(ProviderType.TLevels, false, false)]
        [InlineData(ProviderType.FE | ProviderType.TLevels, true, false)]
        [InlineData(ProviderType.Apprenticeships | ProviderType.TLevels, false, true)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships | ProviderType.TLevels, true, true)]
        public async Task Get_RendersExpectedOutput(
            ProviderType providerType,
            bool expectCoursesCard,
            bool expectApprenticeshipsCard)
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: providerType);

            await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());
            await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());
            await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("PublishedVenueCount").TextContent.Should().Be("3");

                AssertHaveCard("CoursesCard", expectCoursesCard);
                AssertHaveCard("ApprenticeshipsCard", expectApprenticeshipsCard);
                AssertHaveCard("VenuesCard", true);  // Always show venues
            }

            void AssertHaveCard(string testId, bool expectCard)
            {
                var card = doc.GetElementByTestId(testId);

                if (expectCard)
                {
                    card.Should().NotBeNull();
                }
                else
                {
                    card.Should().BeNull();
                }
            }
        }

        [Fact]
        public async Task Get_UnpublishedVenueUploads()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var venueUpload = await TestData.CreateVenueUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: UploadStatus.ProcessedWithErrors);
            //Create some venue upload rows to test new data in UI
            await TestData.CreateVenueUploadRow(venueUpload.VenueUploadId, 2);
            await TestData.CreateVenueUploadRow(venueUpload.VenueUploadId, 3);
            await TestData.CreateVenueUploadRow(venueUpload.VenueUploadId, 4);
            await TestData.CreateVenueUploadRow(venueUpload.VenueUploadId, 5);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("UnpublishedVenueCount").TextContent.Should().Be("4");
            }

        }
    }
}
