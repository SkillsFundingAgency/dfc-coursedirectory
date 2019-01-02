using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCoursePublishModel
    {
        public string CourseName { get; set; }
        public string CourseProviderReference { get; set; }
        //public DeliveryMode DeliveryMode { get; set; }
        public string DeliveryMode { get; set; }

        public string Url { get; set; }
        public decimal Cost { get; set; }
        public string CostDescription { get; set; }
        public bool AdvancedLearnerLoan { get; set; }

        public DurationUnit DurationUnit { get; set; }
        public int DurationValue { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendanceMode { get; set; }

        // Depending which one we are going to use => id or VENUE_ID ; or BOTH
        public int[] VenueIDs { get; set; }
        //public Guid[] VenueIDs { get; set; }
    }
}
