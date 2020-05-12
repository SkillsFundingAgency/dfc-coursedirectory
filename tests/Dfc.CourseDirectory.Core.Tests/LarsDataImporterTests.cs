using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Dfc.CourseDirectory.Core.ReferenceData.Lars;
using Dfc.CourseDirectory.Testing;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests
{
    public class LarsDataImporterTests : DatabaseTestBase
    {
        public LarsDataImporterTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        [SlowTest]
        public Task ImportData() => WithSqlQueryDispatcher(async dispatcher =>
        {
            // Arrange
            var importer = new LarsDataImporter(dispatcher);

            // Act
            await importer.ImportData();

            // Assert
            Assert.Equal(499, await CountSqlRows("LARS.AwardOrgCode"));
            Assert.Equal(42, await CountSqlRows("LARS.Category"));
            Assert.Equal(123, await CountSqlRows("LARS.LearnAimRefType"));
            Assert.Equal(117042, await CountSqlRows("LARS.LearningDelivery"));
            Assert.Equal(81339, await CountSqlRows("LARS.LearningDeliveryCategory"));
            Assert.Equal(17, await CountSqlRows("LARS.SectorSubjectAreaTier1"));
            Assert.Equal(67, await CountSqlRows("LARS.SectorSubjectAreaTier2"));

            Task<int> CountSqlRows(string tableName)
            {
                var query = $"SELECT COUNT(*) FROM {tableName}";

                return dispatcher.Transaction.Connection.QuerySingleAsync<int>(
                    query,
                    transaction: dispatcher.Transaction);
            }
        });
    }
}
