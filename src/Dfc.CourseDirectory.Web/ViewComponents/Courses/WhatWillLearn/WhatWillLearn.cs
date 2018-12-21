using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn
{
    public class WhatWillLearn : ViewComponent
    {
        public IViewComponentResult Invoke(WhatWillLearnModel model)
        {
            return View("~/ViewComponents/Courses/WhatWillLearn/Default.cshtml", model);
        }
    }
}
