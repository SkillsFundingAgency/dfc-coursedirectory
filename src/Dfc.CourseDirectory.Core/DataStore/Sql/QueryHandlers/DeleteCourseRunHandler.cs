using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class DeleteCourseRunHandler : ISqlQueryHandler<DeleteCourseRun, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, DeleteCourseRun query)
        {
            var sql = @$"
DECLARE @Deleted INT

UPDATE Pttcd.CourseRuns SET
    CourseRunStatus = {(int)CourseStatus.Archived},
    UpdatedOn = @DeletedOn,
    UpdatedBy = @DeletedByUserId
WHERE CourseRunId = @CourseRunId
AND CourseId = @CourseId
AND CourseRunStatus NOT IN ({(int)CourseStatus.Archived}, {(int)CourseStatus.Deleted})

SET @Deleted = @@ROWCOUNT

UPDATE Pttcd.Courses
SET CourseStatus = (
    SELECT SUM(DISTINCT(CourseRunStatus)) FROM Pttcd.CourseRuns
    WHERE CourseId = @CourseId
)
WHERE CourseId = @CourseId

SELECT @Deleted Deleted";

            var paramz = new
            {
                query.CourseId,
                query.CourseRunId,
                query.DeletedOn,
                DeletedByUserId = query.DeletedBy.UserId
            };

            var deleted = (await transaction.Connection.QuerySingleAsync<int>(sql, paramz, transaction)) == 1;

            if (deleted)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
