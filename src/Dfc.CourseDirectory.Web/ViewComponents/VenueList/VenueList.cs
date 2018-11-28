using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueList
{
    public class VenueList : ViewComponent
    {
        public IViewComponentResult Invoke(VenueListModel model)
        {
            var actualModel = model ?? new VenueListModel();

           

            return View("~/ViewComponents/VenueList/Default.cshtml", actualModel);
        }
    }
}