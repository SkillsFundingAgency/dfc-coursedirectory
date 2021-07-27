using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateCourseHandler : ISqlQueryHandler<UpdateCourse, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateCourse query)
        {
            var sql = $@"
UPDATE Pttcd.Courses SET
    CourseDescription = @WhoThisCourseIsFor,
    EntryRequirements = @EntryRequirements,
    WhatYoullLearn = @WhatYoullLearn,
    HowYoullLearn = @HowYoullLearn,
    WhatYoullNeed = @WhatYoullNeed,
    HowYoullBeAssessed = @HowYoullBeAssessed,
    WhereNext = @WhereNext,
    UpdatedBy = @UpdatedByUserId,
    UpdatedOn = @UpdatedOn
WHERE CourseId = @CourseId
AND CourseStatus <> {(int)CourseStatus.Archived}

IF @@ROWCOUNT = 0
BEGIN
    SELECT 1 AS Result
    RETURN
END

DECLARE @CourseRunIds Pttcd.GuidIdTable

INSERT INTO @CourseRunIds
SELECT CourseRunId FROM Pttcd.CourseRuns
WHERE CourseId = @CourseId
AND CourseRunStatus = {(int)CourseStatus.Live}

EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @CourseRunIds, @Now = @UpdatedOn

SELECT 0 AS Result";

            var paramz = new
            {
                query.CourseId,
                query.WhoThisCourseIsFor,
                query.EntryRequirements,
                query.WhatYoullLearn,
                query.HowYoullLearn,
                query.WhatYoullNeed,
                query.HowYoullBeAssessed,
                query.WhereNext,
                UpdatedByUserId = query.UpdatedBy.UserId,
                query.UpdatedOn
            };

            var result = await transaction.Connection.QuerySingleAsync<Result>(sql, paramz, transaction);

            if (result == Result.Success)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }

        private enum Result { Success = 0, NotFound = 1 }
    }
}
