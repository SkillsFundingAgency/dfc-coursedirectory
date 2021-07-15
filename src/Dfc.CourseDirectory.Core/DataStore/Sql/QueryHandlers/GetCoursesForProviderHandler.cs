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
    c.WhereNext
FROM Pttcd.Courses c
JOIN @CourseIds x ON c.CourseId = x.Id
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
JOIN @CourseIds x ON cr.CourseId = x.Id
LEFT JOIN Pttcd.Venues v on v.VenueId = cr.VenueId
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

            var courses = await reader.ReadAsync<CourseResult>();

            var courseRuns = (await reader.ReadAsync<CourseRunResult>())
                .GroupBy(r => r.CourseId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var courseRunSubRegions = (await reader.ReadAsync<CourseRunSubRegionResult>())
                .GroupBy(r => r.CourseRunId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.RegionId).AsEnumerable());

            return courses
                .Select(c => new Course()
                {
                    CourseId = c.CourseId,
                    CourseStatus = c.CourseStatus,
                    ProviderId = c.ProviderId,
                    LearnAimRef = c.LearnAimRef,
                    CourseDescription = c.CourseDescription,
                    EntryRequirements = c.EntryRequirements,
                    WhatYoullLearn = c.WhatYoullLearn,
                    HowYoullLearn = c.HowYoullLearn,
                    WhatYoullNeed = c.WhatYoullNeed,
                    HowYoullBeAssessed = c.HowYoullBeAssessed,
                    WhereNext = c.WhereNext,
                    CourseRuns = courseRuns
                        .GetValueOrDefault(c.CourseId, Enumerable.Empty<CourseRunResult>())
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
                            StudyMode = cr.StudyMode,
                            AttendancePattern = cr.AttendancePattern,
                            National = cr.National,
                            SubRegionIds = courseRunSubRegions.GetValueOrDefault(cr.CourseRunId, Enumerable.Empty<string>()).ToArray(),
                            VenueName = cr.VenueName,
                            ProviderVenueRef = cr.ProviderVenueRef
                        })
                        .ToArray()
                })
                .ToArray();
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
