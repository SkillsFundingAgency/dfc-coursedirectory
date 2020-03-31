using System;
using System.Linq;
using System.Reflection;
using Dfc.CourseDirectory.WebV2.Features;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowNoProviderContextAttribute : Attribute
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
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor == null)
            {
                return;
            }

            if (controllerActionDescriptor.MethodInfo.GetCustomAttribute<AllowNoProviderContextAttribute>() != null ||
                controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowNoProviderContextAttribute>() != null)
            {
                return;
            }

            var hasProviderInfoParameter = context.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(ProviderInfo))
                .Any();

            var isRequiresProviderContextController = context.Controller is IRequiresProviderContextController;

            var requiresProviderContext = 
                controllerActionDescriptor.MethodInfo.GetCustomAttribute<RequiresProviderContextAttribute>() != null ||
                isRequiresProviderContextController;

            if (hasProviderInfoParameter || requiresProviderContext)
            {
                var providerContextFeature = context.HttpContext.Features.Get<ProviderContextFeature>();
                var providerContext = providerContextFeature?.ProviderInfo;

                if (providerContext == null)
                {
                    context.Result = new RedirectToActionResult(
                        "Index",
                        "SearchProvider",
                        new { returnUrl = context.HttpContext.Request.GetEncodedUrl() });
                }
                else if (isRequiresProviderContextController)
                {
                    ((IRequiresProviderContextController)context.Controller).ProviderContext = providerContext;
                }
            }
        }
    }
}
