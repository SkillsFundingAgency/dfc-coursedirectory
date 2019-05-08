using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult
{
    public class ApprenticeshipSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ApprenticeshipsSearchResultModel model)
        {



            var actualModel = model ?? new ApprenticeshipsSearchResultModel();

         


            return View("~/ViewComponents/Apprenticeships/ApprenticeshipSearchResult/Default.cshtml", actualModel);
        }
    }
}