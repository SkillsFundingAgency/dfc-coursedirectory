using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueAddressSelectionConfirmation
{
    public class VenueAddressSelectionConfirmation : ViewComponent
    {
        public IViewComponentResult Invoke(VenueAddressSelectionConfirmationModel model)
        {
            var actualModel = model ?? new VenueAddressSelectionConfirmationModel();

            return View("~/ViewComponents/VenueAddressSelectionConfirmation/Default.cshtml", actualModel);
        }
    }
}
