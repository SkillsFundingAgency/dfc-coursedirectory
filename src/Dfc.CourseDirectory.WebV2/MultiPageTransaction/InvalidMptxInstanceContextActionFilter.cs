using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class InvalidMptxInstanceContextActionFilter : IActionFilter
    {
        private static readonly Type _instanceContextType = typeof(MptxInstanceContext<>);
        private static readonly Type _instanceContextWithParentType = typeof(MptxInstanceContext<,>);

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // If any action parameters of type MptxInstanceContext<> or MptxInstanceContext<,> failed binding
            // return an error response

            var instanceContextParameters = context.ActionDescriptor.Parameters
                .Where(t => t.ParameterType.IsGenericType &&
                    (t.ParameterType.GetGenericTypeDefinition() == _instanceContextType ||
                    t.ParameterType.GetGenericTypeDefinition() == _instanceContextWithParentType));

            foreach (var para in instanceContextParameters)
            {
                if (context.ModelState.TryGetValue(para.Name, out var entry) && entry.Errors.Count != 0)
                {
                    context.Result = new BadRequestResult();
                    return;
                }
            }
        }
    }
}
