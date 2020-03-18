using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowNoCurrentProviderAttribute : Attribute
    {
    }

    public class RedirectToProviderSelectionActionFilter : IActionFilter, IOrderedFilter
    {
        public RedirectToProviderSelectionActionFilter()
        {
        }

        public int Order => 9;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor &&
                (controllerActionDescriptor.MethodInfo.GetCustomAttribute<AllowNoCurrentProviderAttribute>() != null ||
                controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowNoCurrentProviderAttribute>() != null))
            {
                return;
            }

            var providerInfoParameters = context.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(ProviderInfo))
                .ToList();

            foreach (var p in providerInfoParameters)
            {
                if (!context.ActionArguments.TryGetValue(p.Name, out var actionArg) || actionArg == null)
                {
                    context.Result = new RedirectToActionResult(
                        "Index",
                        "Home",
                        new { returnUrl = context.HttpContext.Request.GetEncodedUrl() });
                }
            }
        }
    }
}
