using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewModels.EditCourse
{
    public class EditCourseRunViewModel
    {
        public string LearnAimRef { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public StartDateType StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DurationLength { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public List<SelectListItem> Venues { get; set; }
        public Guid? VenueId { get; set; }
        public ChooseRegionModel ChooseRegion { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string CourseProviderReference { get; set; }
        public string Url { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendanceMode { get; set; }
        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string QualificationType { get; set; }
        public bool FlexibleStartDate { get; set; }
        public PublishMode Mode { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public DateTime? CurrentCourseRunDate { get; set; }
        public DateTime ValPastDateRef { get; set; }
        public string ValPastDateMessage { get; set; }
    }
}
