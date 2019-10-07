using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ApprenticeshipBulkUpload.FixError
{
    public class FixError : ViewComponent
    {
        public IViewComponentResult Invoke(PublishApprenticeshipsViewModel model)
        {
            return View("~/ViewComponents/ApprenticeshipBulkUpload/FixError/Default.cshtml", model);
        }
    }
}
