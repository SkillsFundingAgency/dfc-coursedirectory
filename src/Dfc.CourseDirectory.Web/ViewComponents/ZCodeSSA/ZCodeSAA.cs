using System.Collections.Generic;
using Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSSA
{
    public class ZCodeSSA : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
          

            return View("~/ViewComponents/ZCodeSSA/Default.cshtml", new ZCodeSearchResultModel());
        }
    }
}