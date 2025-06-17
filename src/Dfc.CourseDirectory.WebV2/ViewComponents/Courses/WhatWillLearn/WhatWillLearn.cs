using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.WhatWillLearn
{
    public class WhatWillLearn : ViewComponent
    {
        public IViewComponentResult Invoke(WhatWillLearnModel model)
        {
            return View("~/ViewComponents/Courses/WhatWillLearn/Default.cshtml", model);
        }
    }
}
