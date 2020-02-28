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
        [HttpGet("login")]
        public async Task Login(string returnUrl)
        {
            if (returnUrl == null)
            {
                returnUrl = Url.Action("Check", "Home");
            }

            await HttpContext.ChallengeAsync(new AuthenticationProperties() { RedirectUri = returnUrl });
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
