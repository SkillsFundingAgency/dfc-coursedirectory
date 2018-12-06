using Dfc.CourseDirectory.Web.ViewComponents.AddVenueManual;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ManualAddress
{
    public class ManualAddress : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new AddVenueManualModel();

            return View("~/ViewComponents/ManualAddress/Default.cshtml", model);
        }
    }
}
