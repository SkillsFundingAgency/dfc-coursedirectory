using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.Middleware
{
    public class CustomSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey("X-Ncs-Session-Id"))
            {
                var sessionId = Guid.NewGuid().ToString();
                context.Response.Cookies.Append("X-Ncs-Session-Id", sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
            }

            context.Response.Headers.Append("X-Ncs-Session-Id", context.Request.Cookies["X-Ncs-Session-Id"]);

            await _next(context);
        }
    }
}
