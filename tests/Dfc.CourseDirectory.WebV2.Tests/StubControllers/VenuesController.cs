using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class VenuesController : Controller
    {
        [Route("Venues/Add")]
        public IActionResult AddVenue() => Ok();

        [Route("Venues")]
        public IActionResult Index() => Ok();
    }
}
