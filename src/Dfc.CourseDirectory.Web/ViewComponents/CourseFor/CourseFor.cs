using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CourseFor
{
    public class CourseFor : ViewComponent
    {
        public IViewComponentResult Invoke(CourseForModel model)
        {
            return View("~/ViewComponents/CourseFor/Default.cshtml", model);
        }
    }
}
