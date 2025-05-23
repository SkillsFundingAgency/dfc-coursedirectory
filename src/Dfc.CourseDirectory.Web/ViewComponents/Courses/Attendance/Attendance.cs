using Microsoft.AspNetCore.Mvc;

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
