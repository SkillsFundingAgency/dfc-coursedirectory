using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class ContentSecurityPolicyActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result != null)
            {
                var policy = GetPolicy();
                context.Result = new ResultWithCspHeader(context.Result, policy);
            }

            string GetPolicy()
            {
                var defaultSrc = new[]
                {
                    "'self'",
                    "https://rainmaker.tiny.cloud/",
                    "https://www.google-analytics.com/"
                };

                var styleSrc = new[]
                {
                    "'self'",
                    "'unsafe-inline'",
                    "https://cdn.tiny.cloud/",
                    "https://www.googletagmanager.com/",
                    "https://tagmanager.google.com/",
                    "https://fonts.googleapis.com/",
                    "https://cloud.tinymce.com/",
                    "https://cdnjs.cloudflare.com/"
                };

                var fontSrc = new[]
                {
                    "'self'",
                    "data:",
                    "https://fonts.googleapis.com/",
                    "https://fonts.gstatic.com/",
                    "https://cdn.tiny.cloud/"
                };

                var imgSrc = new[]
                {
                    "'self'",
                    "*",
                    "data:",
                    "https://cdn.tiny.cloud/"
                };

                var scriptSrc = new[]
                {
                    "'self'",
                    "'unsafe-eval'",
                    "'unsafe-inline'",
                    "https://cloud.tinymce.com/",
                    "https://cdnjs.cloudflare.com/",
                    "https://www.googletagmanager.com/",
                    "https://tagmanager.google.com/",
                    "https://www.google-analytics.com/",
                    "https://cdn.tiny.cloud/",
                    "https://cdnjs.cloudflare.com/"
                };

                return $"default-src {string.Join(" ", defaultSrc)}; " +
                    $"style-src {string.Join(" ", styleSrc)}; " +
                    $"font-src {string.Join(" ", fontSrc)}; " +
                    $"img-src {string.Join(" ", imgSrc)}; " +
                    $"script-src {string.Join(" ", scriptSrc)}; ";
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        private class ResultWithCspHeader : IActionResult
        {
            public ResultWithCspHeader(IActionResult innerResult, string policy)
            {
                InnerResult = innerResult;
                Policy = policy;
            }

            public IActionResult InnerResult { get; }

            public string Policy { get; }

            public async Task ExecuteResultAsync(ActionContext context)
            {
                await InnerResult.ExecuteResultAsync(context);

                if (context.HttpContext.Response.GetTypedHeaders()?.ContentType.MediaType == "text/html")
                {
                    context.HttpContext.Response.Headers["Content-Security-Policy"] = Policy;
                }
            }
        }
    }
}
