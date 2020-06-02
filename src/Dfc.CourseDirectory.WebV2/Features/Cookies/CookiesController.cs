using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Cookies
{
    [Route("cookies")]
    public class CookiesController : Controller
    {
        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult CookieDetails()
        {
            return View();
        }
    }
}
