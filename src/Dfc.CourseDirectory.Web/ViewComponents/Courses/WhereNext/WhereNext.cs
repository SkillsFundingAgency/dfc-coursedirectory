using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.WhereNext
{
    public class WhereNext : ViewComponent
    {
        public IViewComponentResult Invoke(WhereNextModel model)
        {
            return View("~/ViewComponents/Courses/WhereNext/Default.cshtml", model);
        }
    }
}
