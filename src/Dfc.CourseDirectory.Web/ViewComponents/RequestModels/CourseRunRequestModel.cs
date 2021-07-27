using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class CourseRunRequestModel
    {
        public string LearnAimRefTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRef { get; set; }
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string Url { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public int DurationLength { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendanceMode { get; set; }

        public Guid VenueId { get; set; }


        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }

        public CourseMode CourseMode { get; set; }

        public PublishMode Mode { get; set; }
    }
}
