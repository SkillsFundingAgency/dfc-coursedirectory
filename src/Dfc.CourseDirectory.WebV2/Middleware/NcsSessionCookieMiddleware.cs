using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.Middleware
{
    public class NcsSessionCookieMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        public NcsSessionCookieMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (context.Request.Cookies["ncs_main_session"] == null)
                {
                    var id = Guid.NewGuid().ToString();
                    var options = new CookieOptions()
                    {
                        Secure = true,
                        IsEssential = true,
                        HttpOnly = true,
                    };
                    context.Response.Cookies.Append("ncs_main_session", id, options);
                    context.Response.Headers.Append("X-Ncs-Session-Id", id);
                }
                else
                {
                    context.Response.Headers.Append("X-Ncs-Session-Id", context.Request.Cookies["ncs_main_session"]);
                }
                return Task.FromResult(0);
            });
            await _requestDelegate(context);
        }
    }
}
