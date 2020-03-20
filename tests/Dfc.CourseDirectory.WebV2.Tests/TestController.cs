using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class TestController : Controller
    {
        [AllowAnonymous]
        [Route("tests/empty")]
        public IActionResult Empty() => View("~/TestViews/Empty.cshtml");
    }
}
