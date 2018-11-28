using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueSearch
{
    public class VenueSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new VenueSearchModel();

            return View("~/ViewComponents/VenueSearch/Default.cshtml", model);
        }
    }
}