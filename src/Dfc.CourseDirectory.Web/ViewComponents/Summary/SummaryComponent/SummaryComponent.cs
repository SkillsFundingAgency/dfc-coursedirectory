﻿using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Summary.SummaryComponent
{
    public class SummaryComponent : ViewComponent
    {
        public IViewComponentResult Invoke(SummaryComponentModel model)
        {
            return View("~/ViewComponents/Summary/SummaryComponent/Default.cshtml", model);
        }
    }
}
