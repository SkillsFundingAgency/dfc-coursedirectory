using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public HomeController(IAuthorizationService authorizationService)
        {
            Throw.IfNull(authorizationService, nameof(authorizationService));

            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index(string errmsg, [FromServices] IFeatureFlagProvider featureFlagProvider)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            HttpContext.Session.SetInt32("ProviderSearch", 1);
            HttpContext.Session.SetString("Option", "Home");
            ViewBag.StatusMessage = errmsg;

            ViewBag.HideHeaderBackLink = true;

            var admin = await _authorizationService.AuthorizeAsync(User, "ElevatedUserRole");

            if (admin.Succeeded)
            {
                return RedirectToAction("Dashboard", "HelpdeskDashboard");
            }

            return View("../Provider/Dashboard");
        }

        [AllowDeactivatedProvider]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowDeactivatedProvider]
        public IActionResult Help()
        {
            return View();
        }
    }
}