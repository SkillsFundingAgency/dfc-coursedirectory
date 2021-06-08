using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues
{
    public class VenuesDataManagementHelp : ViewComponent
    {
        public IViewComponentResult Invoke() => View("~/Features/DataManagement/Venues/Help.cshtml");
    }
}
