using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class AddCourseRequestModel
    {
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string Url { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public int DurationLength { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendanceMode { get; set; }
        public bool National { get; set; }

        public Guid[] SelectedVenues { get; set; }


        public string[] SelectedRegions { get; set; }

        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }

        public CourseMode CourseMode { get; set; }
    }
}
