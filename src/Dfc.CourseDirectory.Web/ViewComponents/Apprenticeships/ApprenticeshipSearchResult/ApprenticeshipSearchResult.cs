using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult
{
    public class ApprenticeshipSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ApprenticeshipsSearchResultModel model)
        {
            var actualModel = model ?? new ApprenticeshipsSearchResultModel();

            //stub
            var listOfApprenticeships = new List<ApprenticeShipsSearchResultItemModel>();
           listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
           {
               ApprenticeshipName = "Test Apprenticeship 1"
           });
            listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
            {
                ApprenticeshipName = "Test Apprenticeship 2"
            });

            actualModel.Items = listOfApprenticeships;


            return View("~/ViewComponents/Apprenticeships/ApprenticeshipSearchResult/Default.cshtml", actualModel);
        }
    }
}