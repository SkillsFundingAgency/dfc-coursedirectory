using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
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
            Assert.Equal(1419, Fixture.DatabaseFixture.InMemoryDocumentStore.Frameworks.All.Count);
            Assert.Equal(30, Fixture.DatabaseFixture.InMemoryDocumentStore.ProgTypes.All.Count);
            Assert.Equal(17, Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier1s.All.Count);
            Assert.Equal(67, Fixture.DatabaseFixture.InMemoryDocumentStore.SectorSubjectAreaTier2s.All.Count);
            Assert.Equal(554, Fixture.DatabaseFixture.InMemoryDocumentStore.Standards.All.Count);
            Assert.Equal(75, Fixture.DatabaseFixture.InMemoryDocumentStore.StandardSectorCodes.All.Count);

            Assert.Equal(499, await CountSqlRows("LARS.AwardOrgCode"));
            Assert.Equal(42, await CountSqlRows("LARS.Category"));
            Assert.Equal(123, await CountSqlRows("LARS.LearnAimRefType"));
            Assert.Equal(117108, await CountSqlRows("LARS.LearningDelivery"));
            Assert.Equal(81447, await CountSqlRows("LARS.LearningDeliveryCategory"));
            Assert.Equal(17, await CountSqlRows("LARS.SectorSubjectAreaTier1"));
            Assert.Equal(67, await CountSqlRows("LARS.SectorSubjectAreaTier2"));

            Task<int> CountSqlRows(string tableName)
            {
                var query = $"SELECT COUNT(*) FROM {tableName}";

                return WithSqlQueryDispatcher(dispatcher => dispatcher.Transaction.Connection.QuerySingleAsync<int>(
                    query,
                    transaction: dispatcher.Transaction));
            }
        }
    }
}
