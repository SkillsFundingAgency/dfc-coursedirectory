using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseHandler : ISqlQueryHandler<GetCourse, Course>
    {
        public async Task<Course> Execute(SqlTransaction transaction, GetCourse query)
        {
            var sql = $@"
SELECT
    c.CourseId, c.CourseStatus, c.CreatedOn, c.UpdatedOn, p.ProviderId, p.Ukprn ProviderUkprn, c.LearnAimRef,
    c.CourseDescription, c.EntryRequirements, c.WhatYoullLearn, c.HowYoullLearn, c.WhatYoullNeed, c.HowYoullBeAssessed,
    c.WhereNext, c.DataIsHtmlEncoded,
    lart.LearnAimRefTypeDesc, ld.AwardOrgCode, ld.NotionalNVQLevelv2, ld.LearnAimRefTitle
FROM Pttcd.Courses c
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
JOIN LARS.LearningDelivery ld ON c.LearnAimRef = ld.LearnAimRef
JOIN LARS.LearnAimRefType lart ON ld.LearnAimRefType = lart.LearnAimRefType
WHERE c.CourseId = @CourseId
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
FROM Pttcd.CourseRuns cr
LEFT JOIN Pttcd.Venues v on v.VenueId = cr.VenueId
WHERE cr.CourseId = @CourseId
AND cr.CourseRunStatus <> {(int)CourseStatus.Archived}

SELECT crsr.CourseRunId, crsr.RegionId
FROM Pttcd.CourseRunSubRegions crsr
JOIN Pttcd.CourseRuns cr ON crsr.CourseRunId = cr.CourseRunId
WHERE cr.CourseId = @CourseId
";

            var paramz = new
            {
                query.CourseId
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            return (await CourseMappingHelper.MapCourses(reader)).SingleOrDefault();
        }
    }
}
