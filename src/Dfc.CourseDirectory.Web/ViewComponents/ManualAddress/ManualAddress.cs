using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddress : ViewComponent
    {
        public IViewComponentResult Invoke(ManualAddressModel model)
        {
            return View("~/ViewComponents/ManualAddress/Default.cshtml", model);
        }
    }
}
