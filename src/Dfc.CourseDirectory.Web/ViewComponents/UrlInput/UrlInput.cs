using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.UrlInput
{
    public class UrlInput : ViewComponent
    {
        public IViewComponentResult Invoke(UrlInputModel model)
        {
            return View("~/ViewComponents/UrlInput/Default.cshtml", model);
        }
    }
}
