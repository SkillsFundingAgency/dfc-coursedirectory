using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.Line2;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Line2
{
    public class Line2 : ViewComponent
    {
        public IViewComponentResult Invoke(Line2Model model)
        {
            return View("~/ViewComponents/Courses/Line2/Default.cshtml", model);
        }
    }
}
