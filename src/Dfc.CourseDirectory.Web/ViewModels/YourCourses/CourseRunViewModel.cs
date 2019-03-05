using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels.YourCourses
{
    public class CourseRunViewModel
    {
        public string Id { get; set; }
        public string CourseName { get; set; }
        public string CourseId { get; set; }
        public string StartDate { get; set; }
        public string Url { get; set; }
        public string Cost { get; set; }
        public string Duration { get; set; }
        public string Region { get; set; }
        public string Venue { get; set; }
        public string DeliveryMode { get; set; }
        public string StudyMode { get; set; }
        public string AttendancePattern { get; set; }
    }
}
