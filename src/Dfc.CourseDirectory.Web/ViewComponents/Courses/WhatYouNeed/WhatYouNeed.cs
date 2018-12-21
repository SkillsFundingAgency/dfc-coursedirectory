using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed
{
    public class WhatYouNeed : ViewComponent
    {
        public IViewComponentResult Invoke(WhatYouNeedModel model)
        {
            return View("~/ViewComponents/Courses/WhatYouNeed/Default.cshtml", model);
        }
    }
}
