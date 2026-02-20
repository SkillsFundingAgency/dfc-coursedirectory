using Microsoft.Data.SqlClient;
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
            int LiveStatus = (int)CourseStatus.Live;

            var sql = $@"
                DECLARE @CourseRunIds Pttcd.GuidIdTable

                INSERT INTO @CourseRunIds
                SELECT TOP {query.MaxCourseRunCount} cr.CourseRunId FROM Pttcd.CourseRuns cr
                LEFT JOIN Pttcd.FindACourseIndex i ON cr.CourseRunId = i.CourseRunId
                WHERE cr.CourseRunStatus = @LiveStatus
                AND i.CourseRunId IS NOT NULL
                AND cr.CreatedOn < @CreatedBefore
                AND cr.CreatedOn > @CreatedAfter

                DECLARE @INSER_COUNT INT = ( SELECT COUNT(*) FROM @CourseRunIds )
                IF @INSER_COUNT = 0
                BEGIN
	                INSERT INTO @CourseRunIds
	                SELECT TOP {query.MaxCourseRunCount} cr.CourseRunId FROM Pttcd.CourseRuns cr
	                LEFT JOIN Pttcd.FindACourseIndex i ON cr.CourseRunId = i.CourseRunId
	                WHERE cr.CourseRunStatus = @LiveStatus
	                AND i.CourseRunId IS NOT NULL
	                AND cr.UpdatedOn < @CreatedBefore
	                AND cr.UpdatedOn > @CreatedAfter
                END

                EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds, @Now

                SELECT COUNT(*) FROM @CourseRunIds
                ";

            return transaction.Connection.QuerySingleAsync<int>(
                sql,
                new
                {
                    LiveStatus,
                    query.Now,
                    query.CreatedAfter,
                    query.CreatedBefore
                },
                commandTimeout: 180,
                transaction: transaction);
        }
    }
}
