using Dfc.CourseDirectory.Web.ViewComponents.AddressSelectionConfirmation;
using Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.AddressSelectionConfirmation
{
    public class AddressSelectionConfirmation : ViewComponent
    {
        public IViewComponentResult Invoke(AddressSelectionConfirmationModel model)
        {

            return View("~/ViewComponents/AddressSelectionConfirmation/Default.cshtml", model);
        }
    }
}