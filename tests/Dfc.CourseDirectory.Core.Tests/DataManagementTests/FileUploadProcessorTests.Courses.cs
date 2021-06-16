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
        [Theory]
        [InlineData("online")]
        [InlineData("work based")]
        [InlineData("xxx")]
        public async Task FindVenue_NonClassroomBasedDeliveryMode_ReturnsNull(string deliveryMode)
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue");

            var row = new CsvCourseRow()
            {
                DeliveryMode = deliveryMode,
                VenueName = "my venue"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedButDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "VENUE2"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatchedButNameProvidedAndDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1",
                VenueName = "another name"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatchedAndNameProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1",
                VenueName = "my venue"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result);
        }

        [Fact]
        public async Task FindVenue_NameProvidedButDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "another name"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_NameProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "my venue"
            };

            // Act
            var result = fileUploadProcessor.FindVenue(row, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result);
        }

        [Fact]
        public async Task ProcessCourseFile_AllRecordsValid_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var uploadRows = DataManagementFileHelper.CreateCourseUploadRows(learningAimRef, rowCount: 3).ToArray();
            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedSuccessfully);
                courseUpload.LastValidated.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessCourseFile_RowHasErrors_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(
                // Empty record will always yield errors
                new CsvCourseRow());

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedWithErrors);
                courseUpload.LastValidated.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ValidateCourseUploadRows_RowsHaveNoLarsCode_AreNotGrouped()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, createdBy: user, UploadStatus.Processing);
            var learningAimRef = await TestData.CreateLearningAimRef();

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var rows = DataManagementFileHelper.CreateCourseUploadRows(learningAimRef, rowCount: 2).ToArray();
            rows[0].LarsQan = string.Empty;
            rows[1].LarsQan = string.Empty;

            var uploadRows = rows.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                var (_, rows) = await fileUploadProcessor.ValidateCourseUploadRows(
                    dispatcher,
                    courseUpload.CourseUploadId,
                    provider.ProviderId,
                    uploadRows);

                // Assert
                rows.First().CourseId.Should().NotBe(rows.Last().CourseId);
            });
        }
    }
}
