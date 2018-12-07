using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.EditVenueName
{
    public class EditVenueName : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new EditAddressViewModel();

            return View("~/ViewComponents/EditVenueName/Default.cshtml", model);
        }
    }
}