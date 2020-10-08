using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.BulkUpload
{
    [RequiresProviderContext]
    [Route("bulk-upload")]
    public class BulkUploadController : Controller
    {
        [HttpGet("courses")]
        public IActionResult Courses()
        {
            return View();
        }

        [HttpGet("courses-formatting")]
        public IActionResult CoursesFormatting()
        {
            return View();
        }

        [HttpGet("apprenticeships")]
        public IActionResult Apprenticeships()
        {
            return View();
        }

        [HttpGet("apprenticeships-formatting")]
        public IActionResult ApprenticeshipsFormatting()
        {
            return View();
        }

        [HttpGet("regions")]
        public IActionResult Regions()
        {
            return View();
        }
    }
}