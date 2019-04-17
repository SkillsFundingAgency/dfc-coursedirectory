﻿using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.Web.ViewModels.CourseSummary
{
    public class CourseSummaryViewModel
    {
        //Course Details
        public int ProviderUKPRN { get; set; }
        public Guid? CourseId { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public bool IsValid { get; set; }
        public string QualificationType { get; set; }

        //CourseRun Details
        public int? CourseInstanceId { get; set; }
        public string CourseName { get; set; }
        public Guid? VenueId { get; set; }
        public string VenueName { get; set; }
        public string CourseURL { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public string ProviderCourseID { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
