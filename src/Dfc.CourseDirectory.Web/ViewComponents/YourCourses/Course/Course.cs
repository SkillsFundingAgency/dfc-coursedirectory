using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Web.ViewModels;

namespace Dfc.CourseDirectory.Web.ViewComponents.YourCourses.Course
{
    public class Course : ViewComponent
    {
        public IViewComponentResult Invoke(YourCoursesViewModel model)
        {
            return View("~/ViewComponents/YourCourses/Course/Default.cshtml", model);
        }
    }
}
