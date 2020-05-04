using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class ContentSecurityPolicyActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();

            if (context.Result != null)
            {
                var policy = GetPolicy();
                context.Result = new ResultWithCspHeader(context.Result, policy);
            }

            string GetPolicy()
            {
                var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

                var defaultSrc = new List<string>()
                {
                    "'self'",
                    "https://rainmaker.tiny.cloud/",
                    "https://www.google-analytics.com/"
                };

                var styleSrc = new List<string>()
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

                var fontSrc = new List<string>()
                {
                    "'self'",
                    "data:",
                    "https://fonts.googleapis.com/",
                    "https://fonts.gstatic.com/",
                    "https://cdn.tiny.cloud/"
                };

                var imgSrc = new List<string>()
                {
                    "'self'",
                    "*",
                    "data:",
                    "https://cdn.tiny.cloud/"
                };

                var scriptSrc = new List<string>()
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

                if (env.IsDevelopment())
                {
                    // For BrowserLink
                    defaultSrc.Add("http://localhost:*");
                    defaultSrc.Add("ws://localhost:*");
                    scriptSrc.Add("http://localhost:*");
                }

                return $"default-src {string.Join(" ", defaultSrc)}; " +
                    $"style-src {string.Join(" ", styleSrc)}; " +
                    $"font-src {string.Join(" ", fontSrc)}; " +
                    $"img-src {string.Join(" ", imgSrc)}; " +
                    $"script-src {string.Join(" ", scriptSrc)}; ";
            }
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
                context.HttpContext.Response.OnStarting(() =>
                {
                    if (context.HttpContext.Response.GetTypedHeaders().ContentType?.MediaType == "text/html")
                    {
                        context.HttpContext.Response.Headers["Content-Security-Policy"] = Policy;
                    }

                    return Task.CompletedTask;
                });

                await InnerResult.ExecuteResultAsync(context);
            }
        }
    }
}
