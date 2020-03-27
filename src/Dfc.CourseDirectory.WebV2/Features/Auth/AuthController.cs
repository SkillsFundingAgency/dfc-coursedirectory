using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Auth
{
    [Route("auth")]
    public class AuthController : Controller
    {
        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            if (returnUrl == null || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }

            if (User.Identity.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return new ChallengeResult(new AuthenticationProperties() { RedirectUri = returnUrl });
            }
        }

        [Authorize]
        [AllowDeactivatedProvider]
        [HttpGet("logout")]
        public async Task Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
