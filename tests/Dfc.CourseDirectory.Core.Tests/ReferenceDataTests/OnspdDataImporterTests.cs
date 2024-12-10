using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Onspd;
using FluentAssertions;
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
        public Task ImportData_ReadsCsvAndUpsertsToSql() => WithSqlQueryDispatcher(async sqlDispatcher =>
        {
            // Arrange

            // Create a CSV with 4 records;
            // 2 valid inside England, 1 valid outside of England and one with invalid lat/lng
            var csv = $@"pcds,lat,long,ctry
AB1 2CD,1,-1,{OnspdDataImporter.EnglandCountryId}
BC2 3DE,-2,2,{OnspdDataImporter.EnglandCountryId}
CD3 4EF,3,3,notenglandcountry
DE4 5FG,-99,1,{OnspdDataImporter.EnglandCountryId}";

            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            csvStream.Seek(0L, SeekOrigin.Begin);

            var downloadResponse = new Mock<Response<BlobDownloadInfo>>();

            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: csvStream);

            downloadResponse.SetupGet(mock => mock.Value).Returns(blobDownloadInfo);

            var blobClient = new Mock<BlobClient>();

            blobClient
                .Setup(mock => mock.DownloadAsync())
                .ReturnsAsync(downloadResponse.Object);

            var blobContainerClient = new Mock<BlobContainerClient>();

            blobContainerClient
                .Setup(mock => mock.GetBlobClient(OnspdDataImporter.FileName))
                .Returns(blobClient.Object);

            var blobServiceClient = new Mock<BlobServiceClient>();

            blobServiceClient
                .Setup(mock => mock.GetBlobContainerClient(OnspdDataImporter.ContainerName))
                .Returns(blobContainerClient.Object);

            var importer = new OnspdDataImporter(
                blobServiceClient.Object,
                sqlDispatcher,
                new NullLogger<OnspdDataImporter>());

            // Act
            await importer.ImportData();

            // Assert
            var imported = (await sqlDispatcher.Transaction.Connection.QueryAsync<PostcodeInfo>(
                "select Postcode, Position.Lat Latitude, Position.Long Longitude, InEngland from Pttcd.Postcodes",
                transaction: sqlDispatcher.Transaction)).AsList();

            imported.Should().BeEquivalentTo(new[]
            {
                new PostcodeInfo() { Postcode = "AB1 2CD", Latitude = 1, Longitude = -1, InEngland = true },
                new PostcodeInfo() { Postcode = "BC2 3DE", Latitude = -2, Longitude = 2, InEngland = true },
                new PostcodeInfo() { Postcode = "CD3 4EF", Latitude = 3, Longitude = 3, InEngland = false }
            });
        });
    }
}
