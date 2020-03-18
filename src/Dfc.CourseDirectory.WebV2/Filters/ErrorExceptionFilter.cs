using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class ErrorExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exceptionType = context.Exception.GetType();
            if (exceptionType.IsGenericType && exceptionType.GetGenericTypeDefinition() == typeof(ErrorException<>))
            {
                context.Result = new BadRequestResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
