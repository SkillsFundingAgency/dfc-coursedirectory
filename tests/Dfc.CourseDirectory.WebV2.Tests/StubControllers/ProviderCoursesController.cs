using System;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Tests.StubControllers
{
    public class ProviderCoursesController : Controller
    {
        [HttpGet("ProviderCourses/CourseConfirmationDelete")]
        public IActionResult CourseConfirmationDelete(Guid courseId, Guid courseRunId, string courseName) => Ok();
    }
}
