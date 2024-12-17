using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class AuditAndSyncCourseRunsToIndexHandler : ISqlQueryHandler<AuditAndSyncCourseRunsToIndex, int>
    {
        public Task<int> Execute(SqlTransaction transaction, AuditAndSyncCourseRunsToIndex query)
        {
            var sql = $@"
DECLARE @OrphanedCourseRunIds Pttcd.GuidIdTable

INSERT INTO @OrphanedCourseRunIds
SELECT TOP {query.MaxCourseRunCount} i.CourseRunId
FROM Pttcd.FindACourseIndex i
LEFT JOIN Pttcd.CourseRuns cr ON i.CourseRunId = cr.CourseRunId
WHERE cr.CourseRunId IS NULL

DELETE FROM Pttcd.FindACourseIndex
WHERE CourseRunId IN (SELECT CourseRunId FROM @OrphanedCourseRunIds)

SELECT COUNT(*) FROM @OrphanedCourseRunIds
";
            return transaction.Connection.QuerySingleAsync<int>(
                sql,
                new
                {
                    query.Now
                },
                commandTimeout: 180,
                transaction: transaction);
        }
    }
}
