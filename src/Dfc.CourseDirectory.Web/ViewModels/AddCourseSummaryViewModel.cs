using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCourseSummaryViewModel
    {
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string LearnAimRefTitle { get; set; }

        public string CourseName { get; set; }
        public string CourseId { get; set; }
        public string DeliveryMode { get; set; }
        public string StartDate { get; set; }
        public string Url { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public string AdultEducationBudget { get; set; }
        public string AdvancedLearnerLoan { get; set; }
        public string CourseLength { get; set; }
        public string AttendancePattern { get; set; }
        public string AttendanceTime { get; set; }

        public bool National { get; set; }

        public string WhoIsThisCourseFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string WhereNext { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowAssessed { get; set; }
        public string HowYouWillLearn { get; set; }
        public CourseDeliveryMode? DeliveryModeEnum { get; set; }

        public IEnumerable<string> Venues;
        public IEnumerable<string> Regions;
        public IEnumerable<string> FundingOptions;
    }
}
