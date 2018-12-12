using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearch
{
    public class ProviderSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new ProviderSearchModel();

            return View("~/ViewComponents/ProviderSearch/Default.cshtml", model);
        }
    }
}
