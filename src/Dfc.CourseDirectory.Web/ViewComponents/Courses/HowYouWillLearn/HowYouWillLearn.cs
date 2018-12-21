using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.HowYouWillLearn
{
    public class HowYouWillLearn : ViewComponent
    {
        public IViewComponentResult Invoke(HowYouWillLearnModel model)
        {
            return View("~/ViewComponents/Courses/HowYouWillLearn/Default.cshtml", model);
        }
    }
}
