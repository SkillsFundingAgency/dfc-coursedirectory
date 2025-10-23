using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.Middleware
{
    public class NcsSessionCookieMiddleware
    {
        private const string SessionCookieName = "ncs_main_session";
        private const string SesionHeaderName = "X-Ncs-Session-Id";
        private readonly RequestDelegate _requestDelegate;

        public NcsSessionCookieMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var sessionId = context.Request.Cookies[SessionCookieName];

                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString();
                    var options = new CookieOptions()
                    {
                        Secure = true,
                        IsEssential = true,
                        HttpOnly = true,
                    };
                    context.Response.Cookies.Append(SessionCookieName, sessionId, options);
                }

                context.Response.Headers.Append(SesionHeaderName, sessionId);

                return Task.FromResult(0);
            });
            await _requestDelegate(context);
        }
    }
}
