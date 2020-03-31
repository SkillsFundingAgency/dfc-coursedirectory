using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class TestController : Controller
    {
        [AllowAnonymous]
        [Route("tests/empty-provider-context")]
        public IActionResult EmptyProviderContext() => View("~/TestViews/EmptyProviderContext.cshtml");
    }
}
