using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Components.LarsSearchResult
{
    public class LarsSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(LarsSearchResultModel model)
        {
            return View("~/Components/LarsSearchResult/Default.cshtml", model);
        }
    }
}
