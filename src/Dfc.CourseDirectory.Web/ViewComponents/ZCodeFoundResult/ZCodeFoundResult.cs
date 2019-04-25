using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeFoundResult
{
    public class ZCodeFoundResult : ViewComponent
    {
        public IViewComponentResult Invoke(ZCodeFoundResultModel model)
        {
            var actualModel = model ?? new ZCodeFoundResultModel();

            return View("~/ViewComponents/ZCodeFoundResult/Default.cshtml", actualModel);
        }
    }
}