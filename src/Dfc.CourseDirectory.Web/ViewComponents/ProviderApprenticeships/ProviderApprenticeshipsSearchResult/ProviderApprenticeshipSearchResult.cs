using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult
{
    public class ProviderApprenticeshipSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderApprenticeshipsSearchResultModel model)
        {



            var actualModel = model ?? new ProviderApprenticeshipsSearchResultModel();

         


            return View("~/ViewComponents/ProviderApprenticeships/ProviderApprenticeshipsSearchResult/Default.cshtml", actualModel);
        }
    }
}