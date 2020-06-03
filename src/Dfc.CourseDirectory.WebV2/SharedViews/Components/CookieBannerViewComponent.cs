﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class CookieBannerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() =>
            View("~/SharedViews/Components/CookieBanner.cshtml");
    }
}
