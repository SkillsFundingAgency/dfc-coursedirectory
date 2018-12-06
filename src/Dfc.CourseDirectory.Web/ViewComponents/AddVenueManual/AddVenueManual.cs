using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.AddVenueManual
{
    public class AddVenueManual : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new AddVenueManualModel();

            return View("~/ViewComponents/AddVenueManual/Default.cshtml", model);
        }
    }
}
