using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Error
{
    public class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        [HttpGet("error")]
        public IActionResult Error()
        {
            // If there is no error, return a 404
            // (prevents browsing to this page directly)
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature == null)
            {
                return NotFound();
            }

            var result = View("GenericError");
            result.StatusCode = 500;
            return result;
        }
    }
}
