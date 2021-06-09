using System;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvCourseRow
    {
        public const char SubRegionDelimiter = ';';

        [Index(0), Name("LARS_QAN")]
        public string LarsQan { get; set; }
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

        public static CourseAttendancePattern? ResolveAttendancePattern(string value) => value?.ToLower() switch
        {
            "daytime" => CourseAttendancePattern.Daytime,
            "evening" => CourseAttendancePattern.Evening,
            "weekend" => CourseAttendancePattern.Weekend,
            "day/block release" => CourseAttendancePattern.DayOrBlockRelease,
            _ => (CourseAttendancePattern?)null
        };

        public static CourseDeliveryMode? ResolveDeliveryMode(string value) => value?.ToLower() switch
        {
            "classroom based" => CourseDeliveryMode.ClassroomBased,
            "classroom" => CourseDeliveryMode.ClassroomBased,
            "online" => CourseDeliveryMode.Online,
            "work based" => CourseDeliveryMode.WorkBased,
            "work" => CourseDeliveryMode.WorkBased,
            _ => (CourseDeliveryMode?)null
        };

        public static CourseDurationUnit? ResolveDurationUnit(string value) => value?.ToLower() switch
        {
            "hours" => CourseDurationUnit.Hours,
            "days" => CourseDurationUnit.Days,
            "weeks" => CourseDurationUnit.Weeks,
            "months" => CourseDurationUnit.Months,
            "years" => CourseDurationUnit.Years,
            _ => (CourseDurationUnit?)null
        };

        public static bool? ResolveFlexibleStartDate(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            "" => false,
            _ => null
        };

        public static bool? ResolveNationalDelivery(string value) => value?.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            _ => null
        };

        public static CourseStudyMode? ResolveStudyMode(string value) => value?.ToLower() switch
        {
            "full time" => CourseStudyMode.FullTime,
            "part time" => CourseStudyMode.PartTime,
            "flexible" => CourseStudyMode.Flexible,
            _ => (CourseStudyMode?)null
        };
    }
}
