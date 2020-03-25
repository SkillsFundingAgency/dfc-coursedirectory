﻿using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class AdminProviderContextNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderInfo providerInfo) =>
            View("~/SharedViews/Components/AdminProviderContextNav.cshtml", providerInfo);
    }
}
