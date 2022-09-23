using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.Core.Configuration;
using FluentAssertions;
using FluentAssertions.Execution;
using JustEat.HttpClientInterception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class LarsDataImporterTests : DatabaseTestBase
    {
        public LarsDataImporterTests(DatabaseTestBaseFixture databaseFixture)
            : base(databaseFixture)
        {
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
                Url =
                    "https://submit-learner-data.service.gov.uk/find-a-learning-aim/DownloadData/GetDownloadFileAsync?fileName=published%2F008%2FLearningDelivery_V008_CSV.Zip"
            });

            var importer = new LarsDataImporter(
                client,
                SqlQueryDispatcherFactory,
                CosmosDbQueryDispatcher.Object,
                Clock,
                GetLogger(),
                larsDataSetOption);

            // Act
            await importer.ImportData();

            // Assert
            using (new AssertionScope())
            {
                Fixture.DatabaseFixture.InMemoryDocumentStore.ProgTypes.All.Count.Should().Be(28);
                Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier1s.All.Count.Should().Be(17);
                Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier2s.All.Count.Should().Be(67);
                Fixture.DatabaseFixture.InMemoryDocumentStore.Standards.All.Count.Should().Be(683);
                Fixture.DatabaseFixture.InMemoryDocumentStore.StandardSectorCodes.All.Count.Should().Be(75);

                (await CountSqlRows("LARS.AwardOrgCode")).Should().Be(548);
                (await CountSqlRows("LARS.Category")).Should().Be(63);
                (await CountSqlRows("LARS.LearnAimRefType")).Should().Be(124);
                (await CountSqlRows("LARS.LearningDelivery")).Should().Be(123760);
                (await CountSqlRows("LARS.LearningDeliveryCategory")).Should().Be(88071);
                (await CountSqlRows("LARS.SectorSubjectAreaTier1")).Should().Be(17);
                (await CountSqlRows("LARS.SectorSubjectAreaTier2")).Should().Be(67);
                (await CountSqlRows("Pttcd.Standards")).Should().Be(683);
                (await CountSqlRows("Pttcd.StandardSectorCodes")).Should().Be(75);
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
