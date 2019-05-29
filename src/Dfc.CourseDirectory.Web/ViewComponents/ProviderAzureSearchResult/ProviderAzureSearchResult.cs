
using System;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Web.ViewModels.ProviderSearch;


namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderAzureSearchResult
{
    public class ProviderAzureSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderSearchViewModel model) //ProviderAzureSearchResultModel model)
        {
            return View("~/ViewComponents/ProviderAzureSearchResult/Default.cshtml", model ?? new ProviderSearchViewModel());
        }
    }
}
