using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class OnspdDataImporterTests : DatabaseTestBase
    {
        public OnspdDataImporterTests(Testing.DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task ImportData_ReadsCsvAndUpsertToSql()
        {
            // Arrange
            // Create a CSV with 4 records
            // 2 valid inside England, 1 valid outside of England and one with invalid lat/lng
            var csv = $@"pcds,lat,long,ctry25cd
AB1 2CD,1,-1,{OnspdDataImporter.EnglandCountryId}
BC2 3DE,-2,2,{OnspdDataImporter.EnglandCountryId}
CD3 4EF,3,3,notenglandcountry
DE4 5FG,-99,1,{OnspdDataImporter.EnglandCountryId}";

            var blobClient = new Mock<BlobClient>();
            var blobContainerClient = new Mock<BlobContainerClient>();
            var blobServiceClient = new Mock<BlobServiceClient>();
            var downloadResponse = new Mock<Response<BlobDownloadStreamingResult>>();

            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            csvStream.Seek(0L, SeekOrigin.Begin);

            var blobDownloadInfo = BlobsModelFactory.BlobDownloadStreamingResult(content: csvStream);

            downloadResponse
                .SetupGet(mock => mock.Value).Returns(blobDownloadInfo);

            blobClient
                .Setup(mock => mock.DownloadStreamingAsync(null, CancellationToken.None))
                .ReturnsAsync(downloadResponse.Object);

            blobContainerClient
                .Setup(mock => mock.GetBlobClient(string.Empty))
                .Returns(blobClient.Object);

            blobServiceClient
                .Setup(mock => mock.GetBlobContainerClient(OnspdDataImporter.ContainerName))
                .Returns(blobContainerClient.Object);

            var importer = new OnspdDataImporter(
                blobServiceClient.Object,
                SqlQueryDispatcherFactory,
                new NullLogger<OnspdDataImporter>());

            // Act
            await importer.ManualDataImport(string.Empty);

            // Assert
            var records = new List<PostcodeInfo>();

            await WithSqlQueryDispatcher(async sqlDispatcher =>
            {
                records = (await sqlDispatcher.Transaction.Connection.QueryAsync<PostcodeInfo>(
                    "select Postcode, Position.Lat Latitude, Position.Long Longitude, InEngland from Pttcd.Postcodes",
                    transaction: sqlDispatcher.Transaction)).ToList();
            });

            Assert.Collection(
                records,
                record =>
                {
                    Assert.Equal("AB1 2CD", record.Postcode);
                    Assert.Equal(1, record.Latitude);
                    Assert.Equal(-1, record.Longitude);
                    Assert.True(record.InEngland);
                },
                record =>
                {
                    Assert.Equal("BC2 3DE", record.Postcode);
                    Assert.Equal(-2, record.Latitude);
                    Assert.Equal(2, record.Longitude);
                    Assert.True(record.InEngland);
                },
                record =>
                {
                    Assert.Equal("CD3 4EF", record.Postcode);
                    Assert.Equal(3, record.Latitude);
                    Assert.Equal(3, record.Longitude);
                    Assert.False(record.InEngland);
                });
        }
    }
}
