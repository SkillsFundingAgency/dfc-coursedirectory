using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class AuditAndSyncCoursesToIndexHandler : ISqlQueryHandler<AuditAndSyncCoursesToIndex, int>
    {
        public Task<int> Execute(SqlTransaction transaction, AuditAndSyncCoursesToIndex query)
        {
            var sql = $@"
DECLARE @OrphanedCourseIds Pttcd.GuidIdTable

INSERT INTO @OrphanedCourseIds
SELECT TOP {query.MaxCourseCount} i.CourseId
FROM Pttcd.FindACourseIndex i
LEFT JOIN Pttcd.Courses cr ON i.CourseId = cr.CourseId
WHERE cr.CourseId IS NULL

UPDATE Pttcd.FindACourseIndex
SET Live = false
WHERE CourseId IN (SELECT CourseId FROM @OrphanedCourseIds)

SELECT COUNT(*) FROM @OrphanedCourseIds
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

