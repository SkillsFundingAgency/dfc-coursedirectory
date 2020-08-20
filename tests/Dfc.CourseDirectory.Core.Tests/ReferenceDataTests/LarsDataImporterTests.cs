using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
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
            // Arrange
            var importer = new LarsDataImporter(
                CosmosDbQueryDispatcher.Object,
                Services.GetRequiredService<IServiceScopeFactory>(),
                Clock);

            // Act
            await importer.ImportData();

            // Assert
            using (new AssertionScope())
            {
                Fixture.DatabaseFixture.InMemoryDocumentStore.Frameworks.All.Count.Should().Be(466);
                Fixture.DatabaseFixture.InMemoryDocumentStore.ProgTypes.All.Count.Should().Be(30);
                Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier1s.All.Count.Should().Be(17);
                Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier2s.All.Count.Should().Be(67);
                Fixture.DatabaseFixture.InMemoryDocumentStore.Standards.All.Count.Should().Be(554);
                Fixture.DatabaseFixture.InMemoryDocumentStore.StandardSectorCodes.All.Count.Should().Be(75);

                (await CountSqlRows("LARS.AwardOrgCode")).Should().Be(499);
                (await CountSqlRows("LARS.Category")).Should().Be(42);
                (await CountSqlRows("LARS.LearnAimRefType")).Should().Be(123);
                (await CountSqlRows("LARS.LearningDelivery")).Should().Be(117108);
                (await CountSqlRows("LARS.LearningDeliveryCategory")).Should().Be(81447);
                (await CountSqlRows("LARS.SectorSubjectAreaTier1")).Should().Be(17);
                (await CountSqlRows("LARS.SectorSubjectAreaTier2")).Should().Be(67);
            }
        }

        private Task<int> CountSqlRows(string tableName)
        {
            var query = $"SELECT COUNT(*) FROM {tableName}";

            return WithSqlQueryDispatcher(dispatcher => dispatcher.Transaction.Connection.QuerySingleAsync<int>(query, transaction: dispatcher.Transaction));
        }
    }
}
