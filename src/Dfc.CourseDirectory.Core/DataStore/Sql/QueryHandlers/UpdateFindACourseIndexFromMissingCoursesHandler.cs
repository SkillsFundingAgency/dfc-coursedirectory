using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateFindACourseIndexFromMissingCoursesHandler : ISqlQueryHandler<UpdateFindACourseIndexFromMissingCourses, int>
    {
        public Task<int> Execute(SqlTransaction transaction, UpdateFindACourseIndexFromMissingCourses query)
        {
            var sql = $@"
DECLARE @CourseRunIds Pttcd.GuidIdTable

INSERT INTO @CourseRunIds
SELECT TOP {query.MaxCourseRunCount} cr.CourseRunId FROM Pttcd.CourseRuns cr
LEFT JOIN Pttcd.FindACourseIndex i ON cr.CourseRunId = i.CourseRunId
WHERE cr.CourseRunStatus = {(int)CourseStatus.Live}
AND i.CourseRunId IS NULL
AND cr.CreatedOn < @CreatedBefore
AND cr.CreatedOn > @CreatedAfter

EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds, @Now

SELECT COUNT(*) FROM @CourseRunIds
";

            return transaction.Connection.QuerySingleAsync<int>(
                sql,
                new
                {
                    query.Now,
                    query.CreatedAfter,
                    query.CreatedBefore
                },
                transaction: transaction);
        }
    }
}
