using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCoursePublishModel
    {
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public CourseDeliveryMode? DeliveryMode { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string Url { get; set; }
        public decimal Cost { get; set; }
        public string CostDescription { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public CourseDurationUnit? Id { get; set; }
        public int DurationLength { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendanceMode { get; set; }

        public Guid[] VenueIDs { get; set; }
    }
}
