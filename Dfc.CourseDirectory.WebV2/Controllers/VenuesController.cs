using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Authorize]
    public class VenuesController : Controller
    {
        public IActionResult EditVenue(string Id) => RedirectToAction("Details", "EditVenue", new { venueId = Id });
    }
}
