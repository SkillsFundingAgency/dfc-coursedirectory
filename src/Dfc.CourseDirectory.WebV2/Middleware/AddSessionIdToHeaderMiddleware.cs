using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.Middleware
{
    public class AddSessionIdToHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AddSessionIdToHeaderMiddleware> _logger;

        public AddSessionIdToHeaderMiddleware(RequestDelegate next, ILogger<AddSessionIdToHeaderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation($"Session ID before OnStarting: {context?.Session?.Id}");

                context.Response.OnStarting(() =>
                {
                    _logger.LogInformation($"Session ID at OnStarting: {context?.Session?.Id}");

                    if (!context.Response.Headers.ContainsKey("X-Ncs-Session-Id"))
                    {
                        var sessionId = context?.Session?.Id;

                        if (!string.IsNullOrEmpty(sessionId))
                        {
                            _logger.LogInformation($"Adding session ID: {sessionId}");
                            context.Response.Headers.Append("X-Ncs-Session-Id", sessionId);
                        }
                        else
                        {
                            _logger.LogInformation("No session ID, adding 'no-session'.");
                            context.Response.Headers.Append("X-Ncs-Session-Id", "no-session");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Session ID header already set, skipping modification.");
                    }

                    return Task.CompletedTask;
                });

                if (_next == null)
                {
                    _logger.LogError("RequestDelegate (_next) is null in AddSessionIdToHeaderMiddleware.");
                    throw new InvalidOperationException("RequestDelegate (_next) is null.");
                }
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in AddSessionIdToHeaderMiddleware: {ex.Message}");
                throw;
            }
        }
    }
}
