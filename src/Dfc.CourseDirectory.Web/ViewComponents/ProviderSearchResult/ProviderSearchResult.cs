using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult
{
    public class ProviderSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderSearchResultModel model)
        {
            var actualModel = model ?? new ProviderSearchResultModel();

            return View("~/ViewComponents/ProviderSearchResult/Default.cshtml", actualModel);
        }
    }
}
