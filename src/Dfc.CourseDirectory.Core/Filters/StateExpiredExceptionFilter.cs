using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.Core.Filters
{
    public class StateExpiredExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is StateExpiredException)
            {
                context.Result = new ViewResult()
                {
                    ViewName = "PageExpiredError",
                    StatusCode = StatusCodes.Status409Conflict
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
