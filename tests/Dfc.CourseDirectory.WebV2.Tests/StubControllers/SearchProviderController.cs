using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class SearchProviderController : Controller
    {
        [HttpGet("SearchProvider")]
        public IActionResult Index() => Content("Select Provider");
    }
}
