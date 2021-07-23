using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.BulkUpload
{
    [RequireProviderContext]
    [Route("bulk-upload")]
    public class BulkUploadController : Controller
    {
        [HttpGet("apprenticeships")]
        public IActionResult Apprenticeships() => View();

        [HttpGet("apprenticeships-formatting")]
        public IActionResult ApprenticeshipsFormatting() => View();

        [HttpGet("regions")]
        public IActionResult Regions() => View();
    }
}
