using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Cookies
{
    [Route("cookies")]
    [AllowAnonymous]
    public class CookiesController : Controller
    {
        private const string CookieName = "CookiePolicy";
        private readonly IMediator _mediator;

        public CookiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("details")]
        public IActionResult Details() => View();

        [HttpPost("acceptcookies")]
        [AllowAnonymous]
        public IActionResult AcceptCookies()
        {
            if(Request.Cookies[CookieName] == null)
                Response.Cookies.Append(CookieName, "true");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("settings")]
        public async Task<IActionResult> Settings() => await _mediator.SendAndMapResponse(
            new Settings.Query(),
            command => View(command));

        [HttpPost("settings")]
        public async Task<IActionResult> Settings(Settings.Command command) => await _mediator.SendAndMapResponse(
            command,
            response => response.Match<IActionResult>(
                errors => this.ViewFromErrors(errors),
                success =>
                {
                    TempData["SavedCookieSettings"] = true;
                    return RedirectToAction(nameof(Settings));
                }));
    }
}
