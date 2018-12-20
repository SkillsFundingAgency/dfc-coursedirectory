using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CourseName
{
    public class CourseName : ViewComponent
    {
        public IViewComponentResult Invoke(CourseNameModel model)
        {
            return View("~/ViewComponents/CourseName/Default.cshtml", model);
        }
    }
}
