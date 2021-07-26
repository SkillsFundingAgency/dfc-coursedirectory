using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCoursesForProviderHandler : ISqlQueryHandler<GetCoursesForProvider, IReadOnlyCollection<Course>>
    {
        public async Task<IReadOnlyCollection<Course>> Execute(SqlTransaction transaction, GetCoursesForProvider query)
        {
            var sql = $@"
DECLARE @CourseIds Pttcd.GuidIdTable

INSERT INTO @CourseIds
SELECT c.CourseId
FROM Pttcd.Courses c
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
WHERE p.ProviderId = @ProviderId
AND (c.CourseStatus & @CourseStatusMask) <> 0

SELECT
    c.CourseId, c.CourseStatus, @ProviderId ProviderId, c.LearnAimRef,
    c.CourseDescription, c.EntryRequirements, c.WhatYoullLearn, c.HowYoullLearn, c.WhatYoullNeed, c.HowYoullBeAssessed,
    c.WhereNext, c.DataIsHtmlEncoded,
    lart.LearnAimRefTypeDesc, ld.AwardOrgCode, ld.NotionalNVQLevelv2, ld.LearnAimRefTitle
FROM Pttcd.Courses c
JOIN @CourseIds x ON c.CourseId = x.Id
JOIN LARS.LearningDelivery ld ON c.LearnAimRef = ld.LearnAimRef
JOIN LARS.LearnAimRefType lart ON ld.LearnAimRefType = lart.LearnAimRefType

SELECT
    cr.CourseRunId,
    cr.CourseId,
    cr.CourseRunId,
    cr.CourseRunStatus,
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
FROM Pttcd.CourseRuns cr
JOIN @CourseIds x ON cr.CourseId = x.Id
LEFT JOIN Pttcd.Venues v on v.VenueId = cr.VenueId
AND (cr.CourseRunStatus & @CourseStatusMask) <> 0

SELECT crsr.CourseRunId, crsr.RegionId
FROM Pttcd.CourseRunSubRegions crsr
JOIN Pttcd.CourseRuns cr ON crsr.CourseRunId = cr.CourseRunId
JOIN @CourseIds x ON cr.CourseId = x.Id
";

            var courseStatusMask = query.CourseRunStatuses.Aggregate(CourseStatus.None, (current, status) => current | status);

            var paramz = new
            {
                query.ProviderId,
                CourseStatusMask = courseStatusMask
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);
            return await CourseMappingHelper.MapCourses(reader);
        }
    }
}
