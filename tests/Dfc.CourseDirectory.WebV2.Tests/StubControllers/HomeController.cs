using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class HomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => Content("Home");
    }
}
