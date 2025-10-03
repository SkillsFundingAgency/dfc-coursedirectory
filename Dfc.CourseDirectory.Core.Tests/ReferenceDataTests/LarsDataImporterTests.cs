using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dapper;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using JustEat.HttpClientInterception;
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

            var client = CreateClient();

            var larsDataSetOption = Options.Create(new LarsDataset
            {
                Url = "https://submit-learner-data.service.gov.uk/find-a-learning-aim/DownloadData/GetDownloadFileAsync?fileName=published%2F010%2FLearningDelivery_V010_CSV.Zip"
            });

            var importer = new LarsDataImporter(
                client,
                SqlQueryDispatcherFactory,
                GetLogger(),
                larsDataSetOption,blobServiceClient);

            // Act
            await importer.ImportData();

            // Assert
            using (new AssertionScope())
            {
                (await CountSqlRows("LARS.AwardOrgCode")).Should().BeGreaterThanOrEqualTo(600);
                (await CountSqlRows("LARS.Category")).Should().BeGreaterThanOrEqualTo(75);
                (await CountSqlRows("LARS.LearnAimRefType")).Should().BeGreaterThanOrEqualTo(127);
                (await CountSqlRows("LARS.LearningDelivery")).Should().BeGreaterThanOrEqualTo(130182);
                (await CountSqlRows("LARS.LearningDeliveryCategory")).Should().BeGreaterThanOrEqualTo(91157);
                (await CountSqlRows("LARS.SectorSubjectAreaTier1")).Should().BeGreaterThanOrEqualTo(17);
                (await CountSqlRows("LARS.SectorSubjectAreaTier2")).Should().BeGreaterThanOrEqualTo(67);
            }
        }

        private static HttpClient CreateClient()
        {
            var options = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("findalearningaimbeta.fasst.org.uk")
                .ForPath("DownloadData/GetDownloadFileAsync")
                .ForQuery("fileName=published%2F007%2FLearningDelivery_V007_CSV.Zip")
                .Responds()
                .WithContentHeader("Content-Type", "application/zip")
                .WithContentStream(() =>
                {
                    var resourceName = $"{typeof(LarsDataImporterTests).Namespace}.LARS.zip";
                    return typeof(LarsDataImporterTests).Assembly.GetManifestResourceStream(resourceName);
                })
                .RegisterWith(options);

            return options.CreateHttpClient();
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
