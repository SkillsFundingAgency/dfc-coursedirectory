using Microsoft.AspNetCore.Mvc;

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
