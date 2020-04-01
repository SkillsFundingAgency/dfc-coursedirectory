using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class BulkUploadController : Controller
    {
        [HttpGet("BulkUpload")]
        public IActionResult Index() => Ok();

        [HttpGet("BulkUpload/LandingOptions")]
        public IActionResult LandingOptions() => Ok();
    }
}
