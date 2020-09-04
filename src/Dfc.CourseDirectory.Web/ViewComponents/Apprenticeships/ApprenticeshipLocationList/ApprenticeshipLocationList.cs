using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipLocationList
{
    public class ApprenticeshipLocationList :ViewComponent
    {
        public IViewComponentResult Invoke(ApprenticeshipLocationListModel model)
        {
            return View("~/ViewComponents/Apprenticeships/ApprenticeshipLocationList/Default.cshtml", model);

        }
    }
}
