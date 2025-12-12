using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dapper;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class LarsDataImporterTests : DatabaseTestBase
    {
        private BlobServiceClient blobServiceClient;
        public LarsDataImporterTests(DatabaseTestBaseFixture databaseFixture)
            : base(databaseFixture)
        {
            var mock = new Mock<BlobServiceClient>();
            blobServiceClient = mock.Object;
        }

        [Fact]
        [SlowTest]
        public async Task ImportData()
        {
            // Arrange
            Clock.UtcNow = new DateTime(2020, 8, 28, 0, 0, 0, DateTimeKind.Utc);

            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://findalearningaimbeta.fasst.org.uk/DownloadData/GetDownloadFileAsync/fileName=published%2F007%2FLearningDelivery_V007_CSV.Zip")
            };

            var larsDataSetOption = Options.Create(new LarsDataset
            {
                AwardOrgCodeCsv = "AwardOrgCode.csv",
                CategoryCsv = "Category.csv",
                LearnAimRefTypeCsv = "LearnAimRefType.csv",
                LearningDeliveryCsv = "LearningDelivery.csv",
                LearningDeliveryCategoryCsv = "LearningDeliveryCategory.csv",
                SectorSubjectAreaTier1Csv = "SectorSubjectAreaTier1.csv",
                SectorSubjectAreaTier2Csv = "SectorSubjectAreaTier2.csv",
                Url = "https://submit-learner-data.service.gov.uk",
                UrlSuffix = "/find-a-learning-aim/DownloadData",
                ValidityCsv = "Validity.csv"
            });

            var importer = new LarsDataImporter(
                client,
                SqlQueryDispatcherFactory,
                GetLogger(),
                larsDataSetOption, blobServiceClient);

            // Act
            await importer.ImportData();

            // Assert
            using (new AssertionScope())
            {
                (await CountSqlRows("LARS.AwardOrgCode")).Should().BeGreaterThanOrEqualTo(619);
                (await CountSqlRows("LARS.Category")).Should().BeGreaterThanOrEqualTo(75);
                (await CountSqlRows("LARS.LearnAimRefType")).Should().BeGreaterThanOrEqualTo(130);
                (await CountSqlRows("LARS.LearningDelivery")).Should().BeGreaterThanOrEqualTo(131514);
                (await CountSqlRows("LARS.LearningDeliveryCategory")).Should().BeGreaterThanOrEqualTo(91350);
                (await CountSqlRows("LARS.SectorSubjectAreaTier1")).Should().BeGreaterThanOrEqualTo(17);
                (await CountSqlRows("LARS.SectorSubjectAreaTier2")).Should().BeGreaterThanOrEqualTo(67);
            }
        }

        private static ILogger<LarsDataImporter> GetLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            return loggerFactory.CreateLogger<LarsDataImporter>();
        }

        private Task<int> CountSqlRows(string tableName)
        {
            var query = $"SELECT COUNT(*) FROM {tableName}";

            return WithSqlQueryDispatcher(dispatcher => dispatcher.Transaction.Connection.QuerySingleAsync<int>(query, transaction: dispatcher.Transaction));
        }
    }
}
