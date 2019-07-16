using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CsvCourse : ICsvCourse
    {
        [Display(Name = "LARS_QAN")]
        public string LearnAimRef { get; set; }

        [Display(Name = "WHO_IS_THIS_COURSE_FOR")]
        public string CourseDescription { get; set; }

        [Display(Name = "ENTRY_REQUIREMENTS")]
        public string EntryRequirements { get; set; }

        [Display(Name = "WHAT_YOU_WILL_LEARN")]
        public string WhatYoullLearn { get; set; }

        [Display(Name = "HOW_YOU_WILL_LEARN")]
        public string HowYoullLearn { get; set; }

        [Display(Name = "WHAT_YOU_WILL_NEED_TO_BRING")]
        public string WhatYoullNeed { get; set; }

        [Display(Name = "HOW_YOU_WILL_BE_ASSESSED")]
        public string HowYoullBeAssessed { get; set; }

        [Display(Name = "WHERE_NEXT")]
        public string WhereNext { get; set; }

        [Display(Name = "ADVANCED_LEARNER_OPTION")]
        public string AdvancedLearnerLoan { get; set; }

        [Display(Name = "ADULT_EDUCATION_BUDGET")]
        public string AdultEducationBudget { get; set; }

        [Display(Name = "COURSE_NAME")]
        public string CourseName { get; set; }

        [Display(Name = "ID")]
        public string ProviderCourseID { get; set; }

        [Display(Name = "DELIVERY_MODE")]
        public string DeliveryMode { get; set; }

        [Display(Name = "START_DATE")]
        public string StartDate { get; set; }

        [Display(Name = "FLEXIBLE_START_DATE")]
        public string FlexibleStartDate { get; set; }

        [Display(Name = "VENUE")]
        public string VenueName { get; set; }

        [Display(Name = "NATIONAL_DELIVERY")]
        public string National { get; set; }

        [Display(Name = "REGION")]
        public string Regions { get; set; }

        [Display(Name = "SUB_REGION")]
        public string SubRegions { get; set; }

        [Display(Name = "URL")]
        public string CourseURL { get; set; }

        [Display(Name = "COST")]
        public string Cost { get; set; }

        [Display(Name = "COST_DESCRIPTION")]
        public string CostDescription { get; set; }

        [Display(Name = "DURATION")]
        public string DurationValue { get; set; }

        [Display(Name = "DURATION_UNIT")]
        public string DurationUnit { get; set; }

        [Display(Name = "STUDY_MODE")]
        public string StudyMode { get; set; }

        [Display(Name = "ATTENDANCE_PATTERN")]
        public string AttendancePattern { get; set; }

    }
}
