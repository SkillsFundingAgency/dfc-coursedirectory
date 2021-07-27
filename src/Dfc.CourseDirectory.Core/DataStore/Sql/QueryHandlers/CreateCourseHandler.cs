using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateCourseHandler : ISqlQueryHandler<CreateCourse, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, CreateCourse query)
        {
            var sql = $@"
DECLARE @ProviderUkprn INT

SELECT @ProviderUkprn = Ukprn FROM Pttcd.Providers
WHERE ProviderId = @ProviderId

INSERT INTO Pttcd.Courses (
    CourseId,
    CourseStatus,
    CreatedOn,
    CreatedBy,
    UpdatedOn,
    UpdatedBy,
    LearnAimRef,
    ProviderId,
    ProviderUkprn,
    CourseDescription,
    EntryRequirements,
    WhatYoullLearn,
    HowYoullLearn,
    WhatYoullNeed,
    HowYoullBeAssessed,
    WhereNext
) VALUES (
    @CourseId,
    {(int)CourseStatus.Live},
    @CreatedOn,
    @CreatedByUserId,
    @CreatedOn,
    @CreatedByUserId,
    @LearnAimRef,
    @ProviderId,
    @ProviderUkprn,
    @WhoThisCourseIsFor,
    @EntryRequirements,
    @WhatYoullLearn,
    @HowYoullLearn,
    @WhatYoullNeed,
    @HowYoullBeAssessed,
    @WhereNext
)

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
SELECT
    CourseRunId,
    @CourseId,
    {(int)CourseStatus.Live},
    @CreatedOn,
    @CreatedByUserId,
    @CreatedOn,
    @CreatedByUserId,
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
FROM @CourseRuns

INSERT INTO Pttcd.CourseRunSubRegions (CourseRunId, RegionId)
SELECT CourseRunId, RegionId FROM @SubRegions

DECLARE @CourseRunIds Pttcd.GuidIdTable

INSERT INTO @CourseRunIds SELECT CourseRunId FROM @CourseRuns

EXEC Pttcd.RefreshFindACourseIndex @CourseRunIds = @CourseRunIds, @Now = @CreatedOn";

            var paramz = new
            {
                query.CourseId,
                query.ProviderId,
                query.CreatedOn,
                CreatedByUserId = query.CreatedBy.UserId,
                query.LearnAimRef,
                query.WhoThisCourseIsFor,
                query.EntryRequirements,
                query.WhatYoullLearn,
                query.HowYoullLearn,
                query.WhatYoullNeed,
                query.HowYoullBeAssessed,
                query.WhereNext,
                CourseRuns = TvpHelper.CreateCourseRunsTable(query.CourseRuns.Select(cr => (
                    cr.CourseRunId,
                    cr.CourseName,
                    cr.VenueId,
                    cr.ProviderCourseId,
                    cr.DeliveryMode,
                    cr.FlexibleStartDate,
                    cr.StartDate,
                    cr.CourseUrl,
                    cr.Cost,
                    cr.CostDescription,
                    cr.DurationUnit,
                    cr.DurationValue,
                    cr.StudyMode,
                    cr.AttendancePattern,
                    cr.National))),
                SubRegions = TvpHelper.CreateCourseRunSubRegionsTable(
                    from cr in query.CourseRuns
                    from subRegionId in cr.SubRegionIds ?? Array.Empty<string>()
                    select (cr.CourseRunId, subRegionId))
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
