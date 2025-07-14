using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.WhatYouNeed
{
    public class WhatYouNeed : ViewComponent
    {
        public IViewComponentResult Invoke(WhatYouNeedModel model)
        {
            return View("~/ViewComponents/Courses/WhatYouNeed/Default.cshtml", model);
        }
    }
}
