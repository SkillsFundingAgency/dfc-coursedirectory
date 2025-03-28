﻿using System.Net;
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
        [InlineData(ProviderType.None, false)]
        [InlineData(ProviderType.FE, true)]
        [InlineData(ProviderType.TLevels, false)]
        [InlineData(ProviderType.FE | ProviderType.TLevels, true)]
        public async Task Get_RendersExpectedOutput(
            ProviderType providerType,
            bool expectCoursesCard)
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
    }
}
