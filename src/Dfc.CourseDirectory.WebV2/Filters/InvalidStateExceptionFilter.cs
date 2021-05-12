using Dfc.CourseDirectory.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class InvalidStateExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is InvalidStateException ex)
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<InvalidStateExceptionFilter>();
                logger.LogWarning(ex, ex.Message);

                context.Result = new BadRequestResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
