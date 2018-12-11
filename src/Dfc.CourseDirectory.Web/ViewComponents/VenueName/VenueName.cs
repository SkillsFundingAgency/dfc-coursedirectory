using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueName
{
    public class VenueName : ViewComponent
    {
        public IViewComponentResult Invoke(VenueNameModel model)
        {
            return View("~/ViewComponents/VenueName/Default.cshtml", model);
        }
    }
}
