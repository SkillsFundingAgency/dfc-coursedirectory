using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCoursePublishModel
    {
        public Course Course { get; set; }

        public string CourseName { get; set; }
        public AttendancePattern AttendanceMode { get; set; }
    }
}
