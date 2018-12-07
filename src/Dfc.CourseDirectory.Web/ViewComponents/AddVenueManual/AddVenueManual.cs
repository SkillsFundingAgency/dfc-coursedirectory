using Dfc.CourseDirectory.Web.ViewComponents.ManualAddress;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.AddVenueManual
{
    public class AddVenueManual : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new ManualAddressModel();

            return View("~/ViewComponents/AddVenueManual/Default.cshtml", model);
        }
    }
}
