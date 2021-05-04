using System;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CourseRow
    {
        [Index(0), Name("LARS_QAN")]
        public string LarsQan { get; set; }

        [Index(1), Name("WHO_IS_THIS_COURSE_FOR")]
        public string WhoIsThisCourseFor { get; set; }

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

        [Index(8), Name("ADVANCED_LEARNER_OPTION")]
        public string AdvancedLearnerOption { get; set; }

        [Index(9), Name("ADULT_EDUCATION_BUDGET")]
        public string AdultEducationBudget { get; set; }

        [Index(10), Name("COURSE_NAME")]
        public string CourseName { get; set; }

        [Index(11), Name("ID")]
        public string Id { get; set; }

        [Index(12), Name("DELIVERY_MODE")]
        public string DeliveryMode { get; set; }

        [Index(13), Name("START_DATE")]
        public string StartDate { get; set; }

        [Index(14), Name("FLEXIBLE_START_DATE")]
        public string FlexibleStartDate { get; set; }

        [Index(15), Name("VENUE")]
        public string Venue { get; set; }

        [Index(16), Name("NATIONAL_DELIVERY")]
        public string NationalDelivery { get; set; }

        [Index(17), Name("REGION")]
        public string Region { get; set; }

        [Index(18), Name("SUB_REGION")]
        public string SubRegion { get; set; }

        [Index(19), Name("URL")]
        public string Url { get; set; }

        [Index(20), Name("COST")]
        public string Cost { get; set; }

        [Index(21), Name("COST_DESCRIPTION")]
        public string CostDescription { get; set; }

        [Index(22), Name("DURATION")]
        public string Duration { get; set; }

        [Index(23), Name("DURATION_UNIT")]
        public string DurationUnit { get; set; }

        [Index(24), Name("STUDY_MODE")]
        public string StudyMode { get; set; }

        [Index(25), Name("ATTENDANCE_PATTERN")]
        public string AttendancePattern { get; set; }

        public static CourseRow FromModel(Course course) => new CourseRow()
        {
            LarsQan = course.LarsQan,
            WhoIsThisCourseFor = course.WhoIsThisCourseFor,
            EntryRequirements = course.EntryRequirements,
            //WhatYouWillLearn = course., 
            //HowYouWillLearn, 
            //WhatYouWillNeedToBring, 
            //HowYouWillBeAssessed, 
            //WhereNext, 
            //AdvancedLearnerOption, 
            //AdultEducationBudget,
            //CourseName, 
            //Id,
            //DeliveryMode, 
            //StartDate,
            //FlexibleStartDate,
            //Venue,
            //NationalDelivery,
            //Region, 
            //SubRegion,
            //Url, 
            //Cost, 
            //CostDescription,
            //Duration, 
            //DurationUnit,
            //StudyMode,
            //AttendancePattern,
        };
    }
}
