using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteCoursesNotSyncedSinceHandler : ISqlQueryHandler<DeleteCoursesNotSyncedSince, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, DeleteCoursesNotSyncedSince query)
        {
            var sql = $@"
UPDATE Pttcd.CourseRuns
SET CourseRunStatus = {(int)CourseStatus.Archived}
FROM Pttcd.CourseRuns cr
JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
WHERE c.LastSyncedFromCosmos < @SyncedSince

UPDATE Pttcd.Courses SET CourseStatus = s.CourseStatus
FROM Pttcd.Courses c
JOIN (
    SELECT CourseId, SUM(DISTINCT(CourseRunStatus)) CourseStatus FROM Pttcd.CourseRuns
    GROUP BY CourseId
) s ON c.CourseId = s.CourseId
WHERE c.LastSyncedFromCosmos < @SyncedSince";

            await transaction.Connection.ExecuteAsync(sql, new { query.SyncedSince }, transaction);

            return new Success();
        }
    }
}
