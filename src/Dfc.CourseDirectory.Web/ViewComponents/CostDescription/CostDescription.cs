using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CostDescription
{
    public class CostDescription : ViewComponent
    {
        public IViewComponentResult Invoke(CostDescriptionModel model)
        {
            return View("~/ViewComponents/CostDescription/Default.cshtml", model);
        }
    }
}
