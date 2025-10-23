using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.DataManagement.Venues
{
    public class VenuesDataManagementHelp : ViewComponent
    {
        public IViewComponentResult Invoke() => View("~/Views/VenuesDataManagement/Help.cshtml");
    }
}
