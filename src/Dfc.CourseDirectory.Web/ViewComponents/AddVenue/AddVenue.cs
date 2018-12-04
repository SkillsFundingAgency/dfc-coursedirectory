using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.AddVenue
{
    public class AddVenue : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new AddVenueModel();

            return View("~/ViewComponents/AddVenue/Default.cshtml", model);
        }
    }
}