using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncCosmosCourseSubRegionsToSql
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SyncCosmosCourseSubRegionsToSql(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IServiceScopeFactory serviceScopeFactory)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [FunctionName(nameof(SyncCosmosCourseSubRegionsToSql))]
        [NoAutomaticTrigger]
        [Singleton]
        public Task Run(string input) => _cosmosDbQueryDispatcher.ExecuteQuery(
            new ProcessAllCourses()
            {
                ProcessChunk = async chunk =>
                {
                    var withSubRegions = chunk
                        .Where(c => c.CourseRuns
                        .Any(cr => (cr.SubRegions?.Count() ?? 0) != 0))
                        .ToArray();

                    if (withSubRegions.Length > 0)
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var sqlQueryDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();
                        var transaction = sqlQueryDispatcher.Transaction;

                        var createTableSql = @"
CREATE TABLE #CourseRunSubRegions (
    CourseRunId UNIQUEIDENTIFIER,
    RegionId VARCHAR(9) COLLATE SQL_Latin1_General_CP1_CI_AS
)";

                        await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                        await BulkCopyHelper.WriteRecords(
                            withSubRegions.SelectMany(c => c.CourseRuns.SelectMany(cr => (cr.SubRegions ?? Array.Empty<CourseRunSubRegion>()).Select(r => new
                            {
                                CourseRunId = cr.Id,
                                RegionId = r.Id
                            }))),
                            tableName: "#CourseRunSubRegions",
                            transaction);

                        var mergeSql = @"
MERGE Pttcd.CourseRunSubRegions AS target
USING (
    SELECT CourseRunId, RegionId FROM #CourseRunSubRegions
) AS source
ON target.CourseRunId = source.CourseRunId AND target.RegionId = source.RegionId
WHEN NOT MATCHED THEN
    INSERT (CourseRunId, RegionId) VALUES (source.CourseRunId, source.RegionId);";

                        await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);

                        await transaction.CommitAsync();
                    }
                }
            });
    }
}
