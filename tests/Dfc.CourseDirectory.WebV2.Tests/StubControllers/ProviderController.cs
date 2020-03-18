using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class ProviderController : Controller
    {
        [HttpGet("provider/details")]
        public IActionResult Details(ProviderInfo providerInfo) => Ok();
    }
}
