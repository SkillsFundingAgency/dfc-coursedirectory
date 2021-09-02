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

            //Create some venue upload rows to test new data in UI
            var (venueUpload, _) = await TestData.CreateVenueUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(record => record.IsValid = false);
                    rowBuilder.AddRow(record => record.IsValid = false);
                    rowBuilder.AddRow(record => record.IsValid = false);
                    rowBuilder.AddRow(record => record.IsValid = false);
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("UnpublishedVenueCount").TextContent.Should().Be("4");
                doc.GetElementByTestId("venues-upload-new-link").TextContent.Should().Be("Upload new venue data");
            }
        }

        [Fact]
        public async Task Get_UnpublishedCourseUploads()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            // Create some course upload rows to test new data in UI
            var (courseUpload, _) = await TestData.CreateCourseUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: UploadStatus.ProcessedWithErrors,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record => record.IsValid = false);
                    rowBuilder.AddRow(learnAimRef, record => record.IsValid = false);
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("unpublished-course-count").TextContent.Should().Be("2");
                doc.GetElementByTestId("courses-upload-new-link").TextContent.Should().Be("Upload new course data");
            }
        }

        [Fact]
        public async Task TestVenueUploadInProgress()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted,
                providerType: ProviderType.FE);

            var (venueUpload, _) = await TestData.CreateVenueUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: UploadStatus.Processing,
                rowBuilder =>
                {
                    rowBuilder.AddValidRow();
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("venues-in-progress-link").TextContent.Should().Be("Upload in progress");
            }
        }

        [Fact]
        public async Task Get_HasLiveVenues_DoesRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadVenues").Should().NotBeNull();
        }

        [Fact]
        public async Task Get_NoLiveVenues_DoesNotRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadVenues").Should().BeNull();
        }

        [Fact]
        public async Task Get_HasLiveCourses_DoesRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            await TestData.CreateCourse(provider.ProviderId, createdBy: User.ToUserInfo(), learnAimRef: learnAimRef);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadCourses").Should().NotBeNull();
        }

        [Fact]
        public async Task Get_NoLiveCourses_DoesNotRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadCourses").Should().BeNull();
        }

        [Fact]
        public async Task Get_HasLiveApprenticeships_DoesRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var standard = await TestData.CreateStandard(1234, 1, standardName: "My standard");
            await TestData.CreateApprenticeship(provider.ProviderId, standard, createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadApprenticeships").Should().NotBeNull();
        }

        [Fact]
        public async Task Get_NoLiveApprenticeships_DoesNotRenderDownloadLink()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("DownloadApprenticeships").Should().BeNull();
        }

        [Fact]
        public async Task TestApprenticeshipUploadInProgress()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed,
                providerType: ProviderType.Apprenticeships);

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: UploadStatus.Processing);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("apprenticeship-in-progress-link").TextContent.Should().Be("Upload in progress");
            }
        }

        [Fact]
        public async Task Get_ApprenticeshipQAInProcess_DoesNotRenderApprenticeshipCard()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                apprenticeshipQAStatus: ApprenticeshipQAStatus.InProgress,
                providerType: ProviderType.Apprenticeships);

            var (apprenticeshipUpload, _) = await TestData.CreateApprenticeshipUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: UploadStatus.Processing);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("ApprenticeshipsCard").Should().BeNull();
            }
        }

        [Theory]
        [InlineData(UploadStatus.ProcessedWithErrors)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        public async Task Get_UnpublishedApprenticeshipUploadRows_RendersLink(UploadStatus uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            await TestData.CreateApprenticeshipUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: uploadStatus);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("apprenticeships-unpublished-link").Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        //[InlineData(UploadStatus.Published)]  // TODO Uncomment when we can publish apprenticeship uploads
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_NoUnpublishedApprenticeshipUploadRows_DoesNotRenderLink(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            if (uploadStatus.HasValue)
            {
                await TestData.CreateApprenticeshipUpload(providerId: provider.ProviderId, createdBy: User.ToUserInfo(), uploadStatus: uploadStatus.Value);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            using (new AssertionScope())
            {
                doc.GetElementByTestId("apprenticeships-unpublished-link").Should().BeNull();
            }
        }
    }
}
