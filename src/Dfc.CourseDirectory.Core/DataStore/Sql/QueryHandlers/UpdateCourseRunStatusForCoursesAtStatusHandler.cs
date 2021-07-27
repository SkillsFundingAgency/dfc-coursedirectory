using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateCourseRunStatusForCoursesAtStatusHandler : ISqlQueryHandler<UpdateCourseRunStatusForCoursesAtStatus, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, UpdateCourseRunStatusForCoursesAtStatus query)
        {
            var sql = $@"
DECLARE @CourseRuns Pttcd.GuidIdTable

INSERT INTO @CourseRuns
SELECT cr.CourseRunId FROM
Pttcd.Providers p 
JOIN Pttcd.Courses c WITH (HOLDLOCK) ON p.Ukprn = c.ProviderUkprn 
JOIN Pttcd.CourseRuns cr WITH (HOLDLOCK) ON c.CourseId = cr.CourseId
WHERE ProviderId = @ProviderId
AND cr.CourseRunStatus IN @OldStatuses

UPDATE Pttcd.CourseRuns SET
    CourseRunStatus = @NewStatus,
    UpdatedOn = @DeletedOn,
    UpdatedBy = @DeletedByUserId
FROM Pttcd.CourseRuns cr
JOIN @CourseRuns x ON cr.CourseRunId = x.Id

UPDATE Pttcd.Courses SET
    CourseStatus = s.CourseStatus
FROM Pttcd.Courses c
JOIN (
	SELECT DISTINCT CourseId FROM Pttcd.CourseRuns cr
	JOIN @CourseRuns x ON cr.CourseRunId = x.Id
) x ON c.CourseId = x.CourseId
JOIN (
    SELECT CourseId, SUM(DISTINCT(CourseRunStatus)) CourseStatus FROM Pttcd.CourseRuns
    GROUP BY CourseId
) s ON c.CourseId = s.CourseId";

            var paramz = new
            {
                query.ProviderId,
                query.OldStatuses,
                query.NewStatus
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
