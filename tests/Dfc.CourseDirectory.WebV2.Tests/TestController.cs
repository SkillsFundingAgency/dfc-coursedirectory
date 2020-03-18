using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class TestController : Controller
    {
        [Route("tests/empty")]
        public IActionResult Empty() => View("~/TestViews/Empty.cshtml");
    }
}
