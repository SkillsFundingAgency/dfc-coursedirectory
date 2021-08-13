using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateCourseRunHandler : ISqlQueryHandler<CreateCourseRun, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, CreateCourseRun query)
        {
            var sql = $@"
IF NOT EXISTS (SELECT 1 FROM Pttcd.Courses WHERE CourseId = @CourseId)
BEGIN
    SELECT 1 AS Result
    RETURN
END

INSERT INTO Pttcd.CourseRuns (
    CourseRunId,
    CourseId,
    CourseRunStatus,
    CreatedOn,
    CreatedBy,
    UpdatedOn,
    UpdatedBy,
    CourseName,
    VenueId,
    ProviderCourseId,
    DeliveryMode,
    FlexibleStartDate,
    StartDate,
    CourseWebsite,
    Cost,
    CostDescription,
    DurationUnit,
    DurationValue,
    StudyMode,
    AttendancePattern,
    [National]
)
VALUES (
    @CourseRunId,
    @CourseId,
    {(int)CourseStatus.Live},
    @CreatedOn,
    @CreatedByUserId,
    @CreatedOn,
    @CreatedByUserId,
    @CourseName,
    @VenueId,
    @ProviderCourseId,
    @DeliveryMode,
    @FlexibleStartDate,
    @StartDate,
    @CourseWebsite,
    @Cost,
    @CostDescription,
    @DurationUnit,
    @DurationValue,
    @StudyMode,
    @AttendancePattern,
    @National
)

INSERT INTO Pttcd.CourseRunSubRegions (CourseRunId, RegionId)
SELECT @CourseRunId, Value FROM @SubRegionIds

SELECT 0 AS Result

DECLARE @CourseRunIds Pttcd.GuidIdTable

INSERT INTO @CourseRunIds VALUES (@CourseRunId)

EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @CourseRunIds, @Now = @CreatedOn";

            var paramz = new
            {
                query.CourseId,
                query.CourseRunId,
                query.CourseName,
                query.DeliveryMode,
                query.FlexibleStartDate,
                query.StartDate,
                CourseWebsite = query.CourseUrl,
                query.Cost,
                query.CostDescription,
                query.DurationUnit,
                query.DurationValue,
                query.ProviderCourseId,
                query.National,
                SubRegionIds = TvpHelper.CreateStringTable(query.SubRegionIds ?? Enumerable.Empty<string>()),
                query.VenueId,
                query.AttendancePattern,
                query.StudyMode,
                CreatedByUserId = query.CreatedBy.UserId,
                query.CreatedOn,
            };

            var result = await transaction.Connection.QuerySingleAsync<Result>(sql, paramz, transaction);

            if (result == Result.CourseNotFound)
            {
                return new NotFound();
            }
            else
            {
                return new Success();
            }
        }

        private enum Result { Ok = 0, CourseNotFound = 1 }
    }
}
