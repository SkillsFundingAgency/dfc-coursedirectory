using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Home
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index([FromServices] ICurrentUserProvider currentUserProvider)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Start");
            }

            var currentUser = currentUserProvider.GetCurrentUser();

            if (currentUser.IsAdmin)
            {
                return RedirectToAction("Dashboard", "HelpdeskDashboard");
            }
            else  // Provider user
            {
                return RedirectToAction("Dashboard", "Provider");
            }
        }

        [HttpGet("help")]
        [HttpGet("Home/Help", Order = 99)]
        [AllowDeactivatedProvider]
        public IActionResult Help() => View();

        [HttpGet("privacy")]
        [HttpGet("Home/Privacy", Order = 99)]
        [AllowDeactivatedProvider]
        public IActionResult Privacy() => View();
    }
}
