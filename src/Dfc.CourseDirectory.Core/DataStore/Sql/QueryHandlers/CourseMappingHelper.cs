using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using static System.Net.WebUtility;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    internal static class CourseMappingHelper
    {
        public static async Task<IReadOnlyCollection<Course>> MapCourses(SqlMapper.GridReader reader)
        {
            var courses = await reader.ReadAsync<CourseResult>();

            var courseRuns = (await reader.ReadAsync<CourseRunResult>())
                .GroupBy(r => r.CourseId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var courseRunSubRegions = (await reader.ReadAsync<CourseRunSubRegionResult>())
                .GroupBy(r => r.CourseRunId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.RegionId).AsEnumerable());

            // N.B. We need to normalize HTML-encoded data here. The legacy projects HTML-encoded everything before
            // persisting it in Cosmos. We want to move the HTML encoding to the edge, where it should be done.
            // The existing data synced from Cosmos has a DataIsHtmlEncoded column set to true; read that here and
            // decode the relevant fields if it's set.

            return courses.Select(MapCourse).ToArray();

            Course MapCourse(CourseResult row)
            {
                return new Course()
                {
                    CourseId = row.CourseId,
                    CourseStatus = row.CourseStatus,
                    ProviderId = row.ProviderId,
                    LearnAimRef = row.LearnAimRef,
                    CourseDescription = DecodeIfNecessary(row.CourseDescription),
                    EntryRequirements = DecodeIfNecessary(row.EntryRequirements),
                    WhatYoullLearn = DecodeIfNecessary(row.WhatYoullLearn),
                    HowYoullLearn = DecodeIfNecessary(row.HowYoullLearn),
                    WhatYoullNeed = DecodeIfNecessary(row.WhatYoullNeed),
                    HowYoullBeAssessed = DecodeIfNecessary(row.HowYoullBeAssessed),
                    WhereNext = DecodeIfNecessary(row.WhereNext),
                    CourseRuns = courseRuns
                        .GetValueOrDefault(row.CourseId, Enumerable.Empty<CourseRunResult>())
                        .Select(MapCourseRun)
                        .ToArray()
                };

                string DecodeIfNecessary(string field) => row.DataIsHtmlEncoded ? HtmlDecode(field) : field;
            }

            CourseRun MapCourseRun(CourseRunResult row)
            {
                return new CourseRun()
                {
                    CourseRunId = row.CourseRunId,
                    CourseRunStatus = row.CourseRunStatus,
                    CourseName = DecodeIfNecessary(row.CourseName),
                    VenueId = row.VenueId,
                    ProviderCourseId = DecodeIfNecessary(row.ProviderCourseId),
                    DeliveryMode = row.DeliveryMode,
                    FlexibleStartDate = row.FlexibleStartDate,
                    StartDate = row.StartDate,
                    CourseWebsite = row.CourseWebsite,
                    Cost = row.Cost,
                    CostDescription = DecodeIfNecessary(row.CostDescription),
                    DurationUnit = row.DurationUnit,
                    DurationValue = row.DurationValue,
                    StudyMode = row.StudyMode != 0 ? row.StudyMode : null,  // Normalize 0 to null
                    AttendancePattern = row.AttendancePattern != 0 ? row.AttendancePattern : null,  // Normalize 0 to null
                    National = row.National,
                    SubRegionIds = courseRunSubRegions.GetValueOrDefault(row.CourseRunId, Enumerable.Empty<string>()).ToArray(),
                    VenueName = row.VenueName,
                    ProviderVenueRef = row.ProviderVenueRef
                };

                string DecodeIfNecessary(string field) => row.DataIsHtmlEncoded ? HtmlDecode(field) : field;
            }
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
            public bool DataIsHtmlEncoded { get; set; }
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
            public bool DataIsHtmlEncoded { get; set; }
        }

        private class CourseRunSubRegionResult
        {
            public Guid CourseRunId { get; set; }
            public string RegionId { get; set; }
        }
    }
}
