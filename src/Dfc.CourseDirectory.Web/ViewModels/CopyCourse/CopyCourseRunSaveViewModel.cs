﻿using System;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.CopyCourse
{
    public class CopyCourseRunSaveViewModel
    {
        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public StartDateType StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public Guid? VenueId { get; set; }        
        public bool? National { get; set; }
        public string[] SelectedRegions { get; set; }        
        public string Url { get; set; }
        public string Cost { get; set; }
        public decimal? CostDecimal { get => decimal.TryParse(Cost, out var d) ? d : (decimal?)null; }
        public string CostDescription { get; set; }
        public string DurationLength { get; set; }
        public int? DurationLengthInt { get => int.TryParse(DurationLength, out var durationLength) ? durationLength : (int?)null; }
        public CourseDurationUnit DurationUnit { get; set; }
        public CourseAttendancePattern? AttendanceMode { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public string RefererAbsolutePath { get; set; }
        public CourseType? CourseType { get; set; }
    }
}
