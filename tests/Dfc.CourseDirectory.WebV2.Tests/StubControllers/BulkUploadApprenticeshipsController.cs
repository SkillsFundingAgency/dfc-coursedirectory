using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class BulkUploadApprenticeshipsController : Controller
    {
        [HttpGet("BulkUploadApprenticeships")]
        public IActionResult Index() => Ok();
    }
}
