using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.CopyCourse
{
    public class CopyCourseRunSummaryViewModel
    {
        public string LearnAimRefTitle { get; set; }
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public string StartDate { get; set; }
        public IEnumerable<string> Venues { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public string Url { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseLength { get; set; }
        public AttendancePattern AttendancePattern { get; set; }
        public StudyMode StudyMode { get; set; }
    }
}