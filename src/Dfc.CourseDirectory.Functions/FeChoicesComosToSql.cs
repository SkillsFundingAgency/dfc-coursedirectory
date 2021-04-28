using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Functions
{
    public class FeChoicesComosToSql
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FeChoicesComosToSql(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IServiceScopeFactory serviceScopeFactory)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [FunctionName(nameof(FeChoicesComosToSql))]
        [NoAutomaticTrigger]
        [Singleton]
        public Task Run(string input) => _cosmosDbQueryDispatcher.ExecuteQuery(
            new ProcessAllFeChoices()
            {
                ProcessChunk = async chunk =>
                {
                    var feChoices = chunk.ToArray();
                    using var scope = _serviceScopeFactory.CreateScope();
                    var sqlQueryDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();
                    var transaction = sqlQueryDispatcher.Transaction;
                    var createTableSql = @"
CREATE TABLE #FeChoices (
   Ukprn int NOT NULL,
   LearnerSatisfaction DECIMAL(18,1) NULL,
   EmployerSatisfaction DECIMAL(18,1) NULL,
   CreatedDateTimeUtc DATETIME NOT NULL
)";
                    await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                    await BulkCopyHelper.WriteRecords(
                        feChoices.Select(r => new
                        {
                            Ukprn = r.UKPRN,
                            r.LearnerSatisfaction,
                            r.EmployerSatisfaction,
                            r.CreatedDateTimeUtc
                        }),
                        tableName: "#FeChoices",
                        transaction);
                    var mergeSql = @"
MERGE Pttcd.FeChoices AS target
USING (
   SELECT Ukprn, LearnerSatisfaction, EmployerSatisfaction, CreatedDateTimeUtc FROM #FeChoices
) AS source
ON target.Ukprn = source.Ukprn
WHEN NOT MATCHED THEN
   INSERT (ID, Ukprn, LearnerSatisfaction, EmployerSatisfaction, CreatedDateTimeUtc) VALUES (newid(), source.Ukprn, source.LearnerSatisfaction, source.EmployerSatisfaction,source.CreatedDateTimeUtc);";

                    await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);

                    await transaction.CommitAsync();
                }
            });
    }
}
