using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
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
        public async Task ProcessCourseFile_AllRecordsValid_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, blobServiceClient.Object, Clock);

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var uploadRows = DataManagementFileHelper.CreateCourseUploadRows(rowCount: 3).ToArray();
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

        [Fact(Skip = "No validation implemented yet")]
        public async Task ProcessCourseFile_RowHasErrors_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(SqlQueryDispatcherFactory, blobServiceClient.Object, Clock);

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
    }
}
