using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class LocalUrlActionFilter : IActionFilter
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
                    .Select(p => (parameter: p, attribute: p.ParameterInfo.GetCustomAttribute<LocalUrlAttribute>()))
                    .Where(p => p.attribute != null)
                    .ToList();

                foreach (var (parameter, attribute) in decoratedParameters)
                {
                    context.ActionArguments.TryGetValue(parameter.Name, out var value);

                    var isValid = value != null && value is string str && urlHelper.IsLocalUrl(str);

                    if (!isValid)
                    {
                        context.Result = new BadRequestResult();
                    }
                    else if (attribute.AddToViewData)
                    {
                        var key = attribute.ViewDataKey;
                        ((Controller)context.Controller).ViewData.Add(key, value);
                    }
                }
            }
        }
    }
}
