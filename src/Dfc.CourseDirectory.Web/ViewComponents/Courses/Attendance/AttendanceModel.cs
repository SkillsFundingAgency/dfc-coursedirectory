using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Attendance
{
    public class AttendanceModel
    {
        [Required(ErrorMessage = "Enter Attendance Mode")]
        public AttendancePattern AttendanceMode { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
    }
}
