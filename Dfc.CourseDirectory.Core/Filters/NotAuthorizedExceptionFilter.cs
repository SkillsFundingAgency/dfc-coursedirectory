using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.Core.Filters
{
    public class NotAuthorizedExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is NotAuthorizedException)
            {
                context.Result = new ForbidResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
