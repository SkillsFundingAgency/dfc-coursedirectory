using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed
{
    public class HowAssessed : ViewComponent
    {
        public IViewComponentResult Invoke(HowAssessedModel model)
        {
            return View("~/ViewComponents/Courses/HowAssessed/Default.cshtml", model);
        }
    }
}
