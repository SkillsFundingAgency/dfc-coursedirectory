using Dfc.CourseDirectory.Services.Models.Courses;
using System;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCoursePublishModel
    {
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string Url { get; set; }
        public decimal Cost { get; set; }
        public string CostDescription { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public DurationUnit Id { get; set; }
        public int DurationLength { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendanceMode { get; set; }

        public Guid[] VenueIDs { get; set; }
    }
}
