using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvCourseRow
    {
        public const char SubRegionDelimiter = ';';

        [Index(0), Name("LARS_QAN")]
        public string LearnAimRef { get; set; }
        [Index(1), Name("WHO_THIS_COURSE_IS_FOR")]
        public string WhoThisCourseIsFor { get; set; }
        [Index(2), Name("ENTRY_REQUIREMENTS")]
        public string EntryRequirements { get; set; }
        [Index(3), Name("WHAT_YOU_WILL_LEARN")]
        public string WhatYouWillLearn { get; set; }
        [Index(4), Name("HOW_YOU_WILL_LEARN")]
        public string HowYouWillLearn { get; set; }
        [Index(5), Name("WHAT_YOU_WILL_NEED_TO_BRING")]
        public string WhatYouWillNeedToBring { get; set; }
        [Index(6), Name("HOW_YOU_WILL_BE_ASSESSED")]
        public string HowYouWillBeAssessed { get; set; }
        [Index(7), Name("WHERE_NEXT")]
        public string WhereNext { get; set; }
        [Index(8), Name("COURSE_NAME")]
        public string CourseName { get; set; }
        [Index(9), Name("YOUR_REFERENCE")]
        public string ProviderCourseRef { get; set; }
        [Index(10), Name("DELIVERY_MODE")]
        public string DeliveryMode { get; set; }
        [Index(11), Name("START_DATE")]
        public string StartDate { get; set; }
        [Index(12), Name("FLEXIBLE_START_DATE")]
        public string FlexibleStartDate { get; set; }
        [Index(13), Name("VENUE_NAME")]
        public string VenueName { get; set; }
        [Index(14), Name("YOUR_VENUE_REFERENCE")]
        public string ProviderVenueRef { get; set; }
        [Index(15), Name("NATIONAL_DELIVERY")]
        public string NationalDelivery { get; set; }
        [Index(16), Name("SUB_REGION")]
        public string SubRegions { get; set; }
        [Index(17), Name("COURSE_WEBPAGE")]
        public string CourseWebPage { get; set; }
        [Index(18), Name("COST")]
        public string Cost { get; set; }
        [Index(19), Name("COST_DESCRIPTION")]
        public string CostDescription { get; set; }
        [Index(20), Name("DURATION")]
        public string Duration { get; set; }
        [Index(21), Name("DURATION_UNIT")]
        public string DurationUnit { get; set; }
        [Index(22), Name("STUDY_MODE")]
        public string StudyMode { get; set; }
        [Index(23), Name("ATTENDANCE_PATTERN")]
        public string AttendancePattern { get; set; }

        public static CsvCourseRow FromModel(CourseUploadRow row) => new CsvCourseRow()
        {
            LearnAimRef = row.LarsQan,
            WhoThisCourseIsFor = row.WhoThisCourseIsFor,
            EntryRequirements = row.EntryRequirements,
            WhatYouWillLearn = row.WhatYouWillLearn,
            HowYouWillLearn = row.HowYouWillLearn,
            WhatYouWillNeedToBring = row.WhatYouWillNeedToBring,
            HowYouWillBeAssessed = row.HowYouWillBeAssessed,
            WhereNext = row.WhereNext,
            CourseName = row.CourseName,
            ProviderCourseRef = row.ProviderCourseRef,
            DeliveryMode = row.DeliveryMode,
            StartDate = row.StartDate,
            FlexibleStartDate = row.FlexibleStartDate,
            VenueName = row.VenueName,
            ProviderVenueRef = row.ProviderVenueRef,
            NationalDelivery = row.NationalDelivery,
            SubRegions = row.SubRegions,
            CourseWebPage = row.CourseWebPage,
            Cost = row.Cost,
            CostDescription = row.CostDescription,
            Duration = row.Duration,
            DurationUnit = row.DurationUnit,
            StudyMode = row.StudyMode,
            AttendancePattern = row.AttendancePattern
        };

        public static IEnumerable<CsvCourseRow> FromModel(Course row, IReadOnlyCollection<Region> allRegions) =>
            row.CourseRuns
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.DeliveryMode)
                .Select(courseRun => new CsvCourseRow()
                {
                    LearnAimRef = row.LearnAimRef,
                    WhoThisCourseIsFor = row.CourseDescription,
                    EntryRequirements = row.EntryRequirements,
                    WhatYouWillLearn = row.WhatYoullLearn,
                    HowYouWillLearn = row.HowYoullLearn,
                    WhatYouWillNeedToBring = row.WhatYoullNeed,
                    HowYouWillBeAssessed = row.HowYoullBeAssessed,
                    WhereNext = row.WhereNext,
                    CourseName = courseRun.CourseName,
                    ProviderCourseRef = courseRun.ProviderCourseId,
                    DeliveryMode = ParsedCsvCourseRow.MapDeliveryMode(courseRun.DeliveryMode),
                    StartDate = ParsedCsvCourseRow.MapStartDate(courseRun.StartDate),
                    FlexibleStartDate = ParsedCsvCourseRow.MapFlexibleStartDate(courseRun.FlexibleStartDate),
                    VenueName = courseRun.VenueName,
                    ProviderVenueRef = courseRun.ProviderVenueRef,
                    NationalDelivery = ParsedCsvCourseRow.MapNationalDelivery(courseRun.National),
                    SubRegions = ParsedCsvCourseRow.MapSubRegions(courseRun.SubRegionIds, allRegions),
                    CourseWebPage = courseRun.CourseWebsite,
                    Cost = ParsedCsvCourseRow.MapCost(courseRun.Cost),
                    CostDescription = courseRun.CostDescription,
                    Duration = ParsedCsvCourseRow.MapDuration(courseRun.DurationValue),
                    DurationUnit = ParsedCsvCourseRow.MapDurationUnit(courseRun.DurationUnit),
                    StudyMode = ParsedCsvCourseRow.MapStudyMode(courseRun.StudyMode) ?? "",
                    AttendancePattern = ParsedCsvCourseRow.MapAttendancePattern(courseRun.AttendancePattern) ?? ""
                });

        public static CsvCourseRow[][] GroupRows(IEnumerable<CsvCourseRow> rows) =>
            rows.GroupBy(r => r, new CsvCourseRowCourseComparer())
                .Select(g => g.ToArray())
                .ToArray();

        private class CsvCourseRowCourseComparer : IEqualityComparer<CsvCourseRow>
        {
            public bool Equals(CsvCourseRow x, CsvCourseRow y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                // Don't group together records that have no LARS code
                if (string.IsNullOrEmpty(x.LearnAimRef) || string.IsNullOrEmpty(y.LearnAimRef))
                {
                    return false;
                }

                return
                    x.LearnAimRef == y.LearnAimRef &&
                    x.WhoThisCourseIsFor == y.WhoThisCourseIsFor &&
                    x.EntryRequirements == y.EntryRequirements &&
                    x.WhatYouWillLearn == y.WhatYouWillLearn &&
                    x.HowYouWillLearn == y.HowYouWillLearn &&
                    x.WhatYouWillNeedToBring == y.WhatYouWillNeedToBring &&
                    x.HowYouWillBeAssessed == y.HowYouWillBeAssessed &&
                    x.WhereNext == y.WhereNext;
            }

            public int GetHashCode(CsvCourseRow obj) =>
                HashCode.Combine(
                    obj.LearnAimRef,
                    obj.WhoThisCourseIsFor,
                    obj.EntryRequirements,
                    obj.WhatYouWillLearn,
                    obj.HowYouWillLearn,
                    obj.WhatYouWillNeedToBring,
                    obj.HowYouWillBeAssessed,
                    obj.WhereNext);
        }
    }
}
