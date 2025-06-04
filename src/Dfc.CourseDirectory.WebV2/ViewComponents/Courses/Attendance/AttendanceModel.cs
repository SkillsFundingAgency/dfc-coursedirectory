using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.Attendance
{
    public class AttendanceModel
    {
        public CourseAttendancePattern? AttendanceMode { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
    }
}
