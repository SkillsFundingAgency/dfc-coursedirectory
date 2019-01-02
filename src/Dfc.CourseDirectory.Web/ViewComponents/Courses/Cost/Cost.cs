using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Cost
{
    public class Cost : ViewComponent
    {
        public IViewComponentResult Invoke(CostModel model)
        {
            return View("~/ViewComponents/Courses/Cost/Default.cshtml", model);
        }
    }
}
