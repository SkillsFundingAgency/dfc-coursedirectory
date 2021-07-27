using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateCourseRunHandler : ISqlQueryHandler<UpdateCourseRun, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, UpdateCourseRun query)
        {
            var sql = $@"
UPDATE Pttcd.CourseRuns SET
    CourseName = @CourseName,
    DeliveryMode = @DeliveryMode,
    FlexibleStartDate = @FlexibleStartDate,
    StartDate = @StartDate,
    CourseWebsite = @CourseUrl,
    Cost = @Cost,
    CostDescription = @CostDescription,
    DurationUnit = @DurationUnit,
    DurationValue = @DurationValue,
    ProviderCourseId = @ProviderCourseId,
    [National] = @National,
    VenueId = @VenueId,
    AttendancePattern = @AttendancePattern,
    StudyMode = @StudyMode,
    UpdatedBy = @UpdatedByUserId,
    UpdatedOn = @UpdatedOn
WHERE CourseRunId = @CourseRunId
AND CourseRunStatus = {(int)CourseStatus.Live}

IF @@ROWCOUNT = 0
BEGIN
    SELECT 1 AS Result
    RETURN
END

MERGE Pttcd.CourseRunSubRegions AS target
USING (SELECT Value FROM @SubRegionIds) AS source
ON target.CourseRunId = @CourseRunId AND target.RegionId = source.Value
WHEN NOT MATCHED THEN INSERT (CourseRunId, RegionId) VALUES (@CourseRunId, source.Value)
WHEN NOT MATCHED BY SOURCE AND target.CourseRunId = @CourseRunId THEN DELETE;

DECLARE @CourseRunIds Pttcd.GuidIdTable

INSERT INTO @CourseRunIds VALUES (@CourseRunId)

EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @CourseRunIds, @Now = @UpdatedOn

SELECT 0 AS Result";

            var paramz = new
            {
                query.CourseRunId,
                query.CourseName,
                query.DeliveryMode,
                query.FlexibleStartDate,
                query.StartDate,
                query.CourseUrl,
                query.Cost,
                query.CostDescription,
                query.DurationUnit,
                query.DurationValue,
                query.ProviderCourseId,
                query.National,
                SubRegionIds = TvpHelper.CreateStringTable(query.SubRegionIds ?? Array.Empty<string>()),
                query.VenueId,
                query.AttendancePattern,
                query.StudyMode,
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
