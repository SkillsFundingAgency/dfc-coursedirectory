using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewComponents.PublishCourses.PublishCourseRun
{
    public class PublishCourseRun : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<CourseRun> model)
        {
            return View("~/ViewComponents/PublishCourses/PublishCourseRun/Default.cshtml", model);
        }
    }
}
