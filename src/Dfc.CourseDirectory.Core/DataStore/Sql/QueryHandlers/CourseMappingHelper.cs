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

           

            return courses.Select(MapCourse).ToArray();

            Course MapCourse(CourseResult row)
            {
                return new Course()
                {
                    CourseId = row.CourseId,
                    CreatedOn = row.CreatedOn,
                    UpdatedOn = row.UpdatedOn,
                    ProviderId = row.ProviderId,
                    ProviderUkprn = row.ProviderUkprn,
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
                        .ToArray(),
                    LearnAimRefTypeDesc = row.LearnAimRefTypeDesc,
                    AwardOrgCode = row.AwardOrgCode,
                    NotionalNVQLevelv2 = row.NotionalNVQLevelv2,
                    LearnAimRefTitle = row.LearnAimRefTitle
                };

                string DecodeIfNecessary(string field) => row.DataIsHtmlEncoded != false ? HtmlDecode(field) : field;
            }

            CourseRun MapCourseRun(CourseRunResult row)
            {
                var courseRun = new CourseRun()
                {
                    CourseRunId = row.CourseRunId,
                    CourseRunStatus = row.CourseRunStatus,
                    CreatedOn = row.CreatedOn,
                    UpdatedOn = row.UpdatedOn,
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

                // We have some bad data where fields are populated when they shouldn't be for the delivery mode. Fix it up here

                if (courseRun.DeliveryMode != CourseDeliveryMode.ClassroomBased)
                {
                    courseRun.VenueId = null;
                    courseRun.VenueName = null;
                    courseRun.ProviderVenueRef = null;
                    courseRun.StudyMode = null;
                    courseRun.AttendancePattern = null;
                }

                if (courseRun.DeliveryMode != CourseDeliveryMode.WorkBased)
                {
                    courseRun.National = null;
                    courseRun.SubRegionIds = Array.Empty<string>();
                }

                return courseRun;

                string DecodeIfNecessary(string field) => row.DataIsHtmlEncoded != false ? HtmlDecode(field) : field;
            }
        }

        private class CourseResult
        {
            public Guid CourseId { get; set; }
            public CourseStatus CourseStatus { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime? UpdatedOn { get; set; }
            public Guid ProviderId { get; set; }
            public int ProviderUkprn { get; set; }
            public string LearnAimRef { get; set; }
            public string CourseDescription { get; set; }
            public string EntryRequirements { get; set; }
            public string WhatYoullLearn { get; set; }
            public string HowYoullLearn { get; set; }
            public string WhatYoullNeed { get; set; }
            public string HowYoullBeAssessed { get; set; }
            public string WhereNext { get; set; }
            public bool? DataIsHtmlEncoded { get; set; }
            public string LearnAimRefTypeDesc { get; set; }
            public string AwardOrgCode { get; set; }
            public string NotionalNVQLevelv2 { get; set; }
            public string LearnAimRefTitle { get; set; }
        }

        private class CourseRunResult
        {
            public Guid CourseId { get; set; }
            public Guid CourseRunId { get; set; }
            public CourseStatus CourseRunStatus { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime? UpdatedOn { get; set; }
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
            public bool? DataIsHtmlEncoded { get; set; }
        }

        private class CourseRunSubRegionResult
        {
            public Guid CourseRunId { get; set; }
            public string RegionId { get; set; }
        }
    }
}
