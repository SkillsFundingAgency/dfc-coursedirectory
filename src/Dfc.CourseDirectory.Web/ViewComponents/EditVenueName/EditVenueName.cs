using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueName
{
    public class EditVenueName : ViewComponent
    {
        public IViewComponentResult Invoke(EditVenueNameModel model)
        {
            return View("~/ViewComponents/EditVenueName/Default.cshtml", model);
        }
    }
}