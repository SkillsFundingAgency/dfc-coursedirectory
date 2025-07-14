using Dfc.CourseDirectory.Core.Filters;
using Dfc.CourseDirectory.Core.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
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
                return RedirectToAction("Index", "ProviderDashboard");
            }
        }

        [HttpGet("contact")]
        [AllowDeactivatedProvider]
        public IActionResult Contact() => View();

        [HttpGet("Home/Help")]
        [AllowDeactivatedProvider]
        public IActionResult Help() => RedirectToAction(nameof(Contact));

        [HttpGet("accessibility")]
        [AllowDeactivatedProvider]
        public IActionResult Accessibility() => View();

        [HttpGet("privacy")]
        [HttpGet("Home/Privacy", Order = 99)]
        [AllowDeactivatedProvider]
        public IActionResult Privacy() => View();
    }
}
