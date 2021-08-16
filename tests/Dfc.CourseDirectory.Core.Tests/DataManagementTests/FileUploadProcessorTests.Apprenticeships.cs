using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public partial class FileUploadProcessorTests
    {
        [Fact]
        public async Task FindVenue_RowHasVenueIdHintf_ReturnsVenueMatchingHint()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory),
                new ExecuteImmediatelyBackgroundWorkScheduler(Fixture.ServiceScopeFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "xxx"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid(), venueIdHint: venue.VenueId);

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }
    }
}
