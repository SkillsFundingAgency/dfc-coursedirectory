using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Attendance
{
    public class Attendance : ViewComponent
    {
        public IViewComponentResult Invoke(AttendanceModel model)
        {
            return View("~/ViewComponents/Courses/Attendance/Default.cshtml", model);
        }
    }
}
