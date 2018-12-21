using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CostInput
{
    public class CostInput : ViewComponent
    {
        public IViewComponentResult Invoke(CostInputModel model)
        {
            return View("~/ViewComponents/CostInput/Default.cshtml", model);
        }
    }
}
