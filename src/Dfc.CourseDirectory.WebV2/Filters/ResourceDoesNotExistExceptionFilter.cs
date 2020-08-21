using Dfc.CourseDirectory.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class ResourceDoesNotExistExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ResourceDoesNotExistException)
            {
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
