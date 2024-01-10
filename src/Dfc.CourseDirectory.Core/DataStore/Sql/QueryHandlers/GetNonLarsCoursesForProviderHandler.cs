using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetNonLarsCoursesForProviderHandler : ISqlQueryHandler<GetNonLarsCoursesForProvider, IReadOnlyCollection<Course>>
    {
        public async Task<IReadOnlyCollection<Course>> Execute(SqlTransaction transaction, GetNonLarsCoursesForProvider query)
        {
            var sql = $@"
SELECT
    c.CourseId, c.CreatedOn, c.UpdatedOn, c.ProviderId, c.ProviderUkprn, c.LearnAimRef,
    c.CourseDescription, c.EntryRequirements, c.WhatYoullLearn, c.HowYoullLearn, c.WhatYoullNeed, c.HowYoullBeAssessed,
    c.WhereNext, c.DataIsHtmlEncoded, c.CourseType
FROM Pttcd.Courses c
WHERE c.ProviderId = @ProviderId 
AND (LearnAimRef IS NULL OR LearnAimRef = '')
AND c.CourseStatus <> {(int)CourseStatus.Archived}

SELECT
    cr.CourseRunId,
    cr.CourseId,
    cr.CourseRunId,
    cr.CourseRunStatus,
    cr.CreatedOn,
    cr.UpdatedOn,
    cr.CourseName,
    cr.VenueId,
    cr.ProviderCourseId,
    cr.DeliveryMode,
    cr.FlexibleStartDate,
    cr.StartDate,
    cr.CourseWebsite,
    cr.Cost,
    cr.CostDescription,
    cr.DurationUnit,
    cr.DurationValue,
    cr.StudyMode,
    cr.AttendancePattern,
    cr.[National],
    cr.DataIsHtmlEncoded,
    v.VenueName,
    v.ProviderVenueRef
FROM Pttcd.Courses c
JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
LEFT JOIN Pttcd.Venues v on v.VenueId = cr.VenueId
WHERE c.ProviderId = @ProviderId
AND (LearnAimRef IS NULL OR LearnAimRef = '')
AND cr.CourseRunStatus = {(int)CourseStatus.Live}

SELECT crsr.CourseRunId, crsr.RegionId
FROM Pttcd.CourseRunSubRegions crsr
JOIN Pttcd.CourseRuns cr ON crsr.CourseRunId = cr.CourseRunId
JOIN Pttcd.Courses c ON cr.CourseId = c.CourseId
WHERE c.ProviderId = @ProviderId
AND (c.LearnAimRef IS NULL OR c.LearnAimRef = '')
AND cr.CourseRunStatus = {(int)CourseStatus.Live}
";

            var paramz = new
            {
                query.ProviderId
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);
            return await CourseMappingHelper.MapCourses(reader);
        }
    }
}
