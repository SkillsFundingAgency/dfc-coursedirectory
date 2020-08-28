using System;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class LarsDataImporterTests : DatabaseTestBase
    {
        public LarsDataImporterTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        [SlowTest]
        public async Task ImportData()
        {
            Clock.UtcNow = new DateTime(2020, 8, 28, 0, 0, 0, DateTimeKind.Utc);

            // Arrange
            var importer = new LarsDataImporter(
                CosmosDbQueryDispatcher.Object,
                Services.GetRequiredService<IServiceScopeFactory>(),
                Clock,
                GetLogger());

            // Act
            await importer.ImportData();

            // Assert
            using (new AssertionScope())
            {
                Fixture.DatabaseFixture.InMemoryDocumentStore.Frameworks.All.Count.Should().Be(0); // All expired on 31 July 2020
                Fixture.DatabaseFixture.InMemoryDocumentStore.ProgTypes.All.Count.Should().Be(28);
                Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier1s.All.Count.Should().Be(17);
                Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier2s.All.Count.Should().Be(67);
                Fixture.DatabaseFixture.InMemoryDocumentStore.Standards.All.Count.Should().Be(589);
                Fixture.DatabaseFixture.InMemoryDocumentStore.StandardSectorCodes.All.Count.Should().Be(75);

                (await CountSqlRows("LARS.AwardOrgCode")).Should().Be(502);
                (await CountSqlRows("LARS.Category")).Should().Be(44);
                (await CountSqlRows("LARS.LearnAimRefType")).Should().Be(122);
                (await CountSqlRows("LARS.LearningDelivery")).Should().Be(117730);
                (await CountSqlRows("LARS.LearningDeliveryCategory")).Should().Be(82221);
                (await CountSqlRows("LARS.SectorSubjectAreaTier1")).Should().Be(17);
                (await CountSqlRows("LARS.SectorSubjectAreaTier2")).Should().Be(67);
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
