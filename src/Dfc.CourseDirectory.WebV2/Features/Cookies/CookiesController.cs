using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Cookies
{
    [Route("cookies")]
    public class CookiesController : Controller
    {
        private const string CookieName = "CookiePolicy";
        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult CookieDetails()
        {
            return View();
        }

        [HttpPost("acceptcookies")]
        [AllowAnonymous]
        public IActionResult AcceptCookies()
        {
            if(Request.Cookies[CookieName] == null)
                Response.Cookies.Append(CookieName, "true");

            return RedirectToAction("Index", "Home");
        }
    }
}
