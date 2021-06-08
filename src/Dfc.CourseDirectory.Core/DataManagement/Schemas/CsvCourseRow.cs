using System;
using CsvHelper.Configuration.Attributes;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvCourseRow
    {
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
        public string YourReference { get; set; }
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
    }
}
