using System;
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
    public class GetCourseHandler : ISqlQueryHandler<GetCourse, Course>
    {
        public async Task<Course> Execute(SqlTransaction transaction, GetCourse query)
        {
            var sql = $@"
SELECT
    c.CourseId, c.CourseStatus, p.ProviderId, c.LearnAimRef,
    c.CourseDescription, c.EntryRequirements, c.WhatYoullLearn, c.HowYoullLearn, c.WhatYoullNeed, c.HowYoullBeAssessed,
    c.WhereNext
FROM Pttcd.Courses c
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
WHERE c.CourseId = @CourseId
AND c.CourseStatus <> {(int)CourseStatus.Archived}

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

            var course = await reader.ReadSingleOrDefaultAsync<CourseResult>();

            if (course == null)
            {
                return null;
            }

            var courseRuns = await reader.ReadAsync<CourseRunResult>();

            var courseRunSubRegions = (await reader.ReadAsync<CourseRunSubRegionResult>())
                .GroupBy(r => r.CourseRunId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.RegionId).AsEnumerable());

            return new Course()
            {
                CourseId = course.CourseId,
                CourseStatus = course.CourseStatus,
                ProviderId = course.ProviderId,
                LearnAimRef = course.LearnAimRef,
                CourseDescription = course.CourseDescription,
                EntryRequirements = course.EntryRequirements,
                WhatYoullLearn = course.WhatYoullLearn,
                HowYoullLearn = course.HowYoullLearn,
                WhatYoullNeed = course.WhatYoullNeed,
                HowYoullBeAssessed = course.HowYoullBeAssessed,
                WhereNext = course.WhereNext,
                CourseRuns = courseRuns
                    .Select(cr => new CourseRun()
                    {
                        CourseRunId = cr.CourseRunId,
                        CourseRunStatus = cr.CourseRunStatus,
                        CourseName = cr.CourseName,
                        VenueId = cr.VenueId,
                        ProviderCourseId = cr.ProviderCourseId,
                        DeliveryMode = cr.DeliveryMode,
                        FlexibleStartDate = cr.FlexibleStartDate,
                        StartDate = cr.StartDate,
                        CourseWebsite = cr.CourseWebsite,
                        Cost = cr.Cost,
                        CostDescription = cr.CostDescription,
                        DurationUnit = cr.DurationUnit,
                        DurationValue = cr.DurationValue,
                        StudyMode = cr.StudyMode != 0 ? cr.StudyMode : null,  // Normalize 0 to null
                        AttendancePattern = cr.AttendancePattern != 0 ? cr.AttendancePattern : null,  // Normalize 0 to null
                        National = cr.National,
                        SubRegionIds = courseRunSubRegions.GetValueOrDefault(cr.CourseRunId, Enumerable.Empty<string>()).ToArray(),
                        VenueName = cr.VenueName,
                        ProviderVenueRef = cr.ProviderVenueRef
                    })
                    .ToArray()
            };
        }

        private class CourseResult
        {
            public Guid CourseId { get; set; }
            public CourseStatus CourseStatus { get; set; }
            public Guid ProviderId { get; set; }
            public string LearnAimRef { get; set; }
            public string CourseDescription { get; set; }
            public string EntryRequirements { get; set; }
            public string WhatYoullLearn { get; set; }
            public string HowYoullLearn { get; set; }
            public string WhatYoullNeed { get; set; }
            public string HowYoullBeAssessed { get; set; }
            public string WhereNext { get; set; }
        }

        private class CourseRunResult
        {
            public Guid CourseId { get; set; }
            public Guid CourseRunId { get; set; }
            public CourseStatus CourseRunStatus { get; set; }
            public string CourseName { get; set; }
            public Guid? VenueId { get; set; }
            public string ProviderCourseId { get; set; }
            public CourseDeliveryMode DeliveryMode { get; set; }
            public bool FlexibleStartDate { get; set; }
            public DateTime? StartDate { get; set; }
            public string CourseWebsite { get; set; }
            public decimal? Cost { get; set; }
            public string CostDescription { get; set; }
            public CourseDurationUnit DurationUnit { get; set; }
            public int? DurationValue { get; set; }
            public CourseStudyMode? StudyMode { get; set; }
            public CourseAttendancePattern? AttendancePattern { get; set; }
            public bool? National { get; set; }
            public string VenueName { get; set; }
            public string ProviderVenueRef { get; set; }
        }

        private class CourseRunSubRegionResult
        {
            public Guid CourseRunId { get; set; }
            public string RegionId { get; set; }
        }
    }
}
