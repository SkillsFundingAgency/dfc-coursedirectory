using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueAddress
{
    public class EditVenueAddress : ViewComponent
    {
        public IViewComponentResult Invoke(EditVenueAddressModel model)
        {
            return View("~/ViewComponents/EditVenueAddress/Default.cshtml", model);
        }
    }
}