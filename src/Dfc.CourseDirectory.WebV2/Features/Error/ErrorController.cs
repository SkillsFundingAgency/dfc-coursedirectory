using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.Features.Error
{
    [AllowAnonymous]
    [Route("error")]
    public class ErrorController : Controller
    {
        public IActionResult Error(
            int? code,
            [FromServices] HostingOptions hostingOptions,
            [FromServices] ILoggerFactory loggerFactory)
        {
            // If there is no error, return a 404
            // (prevents browsing to this page directly)
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            if (exceptionHandlerPathFeature == null && statusCodeReExecuteFeature == null)
            {
                return NotFound();
            }

            if (exceptionHandlerPathFeature != null)
            {
                var logger = loggerFactory.CreateLogger<ErrorController>();
                logger.LogError(exceptionHandlerPathFeature.Error, exceptionHandlerPathFeature.Error.Message);
            }

            var statusCode = code ?? 500;

            // Treat Forbidden as NotFound so we don't give away our internal URLs
            if (code == 403 && hostingOptions.RewriteForbiddenToNotFound)
            {
                statusCode = 404;
            }

            var viewName = statusCode == 404 ? "NotFound" : "GenericError";
            var result = View(viewName);
            result.StatusCode = statusCode;
            return result;
        }
    }
}
