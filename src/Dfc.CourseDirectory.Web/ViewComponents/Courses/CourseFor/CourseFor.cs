using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor
{
    public class CourseFor : ViewComponent
    {
        public IViewComponentResult Invoke(CourseForModel model)
        {
            return View("~/ViewComponents/Courses/CourseFor/Default.cshtml", model);
        }
    }
}
