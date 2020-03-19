using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class ValidateLocalUrlActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                var urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(context);

                var decoratedParameters = controllerActionDescriptor.Parameters
                    .OfType<ControllerParameterDescriptor>()
                    .Where(p => p.ParameterInfo.GetCustomAttribute<ValidateLocalUrlAttribute>() != null)
                    .ToList();

                foreach (var param in decoratedParameters)
                {
                    context.ActionArguments.TryGetValue(param.Name, out var value);

                    var isValid = value != null && value is string str && urlHelper.IsLocalUrl(str);

                    if (!isValid)
                    {
                        context.Result = new BadRequestResult();
                    }
                }
            }
        }
    }
}
