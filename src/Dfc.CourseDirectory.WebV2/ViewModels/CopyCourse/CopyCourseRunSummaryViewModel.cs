using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2.ViewModels.CopyCourse
{
    public class CopyCourseRunSummaryViewModel
    {
        public string LearnAimRefTitle { get; set; }
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string StartDate { get; set; }
        public IEnumerable<string> Venues { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public string Url { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseLength { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public CourseStudyMode? StudyMode { get; set; }        
        public bool NonLarsCourse { get; set; }
    }
}
