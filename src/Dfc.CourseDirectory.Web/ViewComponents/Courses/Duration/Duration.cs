using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class Duration : ViewComponent
    {
        public IViewComponentResult Invoke(DurationModel model)
        {
            return View("~/ViewComponents/Courses/Duration/Default.cshtml", model);
        }
    }
}
