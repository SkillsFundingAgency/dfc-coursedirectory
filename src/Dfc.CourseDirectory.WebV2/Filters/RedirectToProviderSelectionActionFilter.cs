using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowNoProviderContextAttribute : Attribute
    {
    }

    public class RedirectToProviderSelectionActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        public RedirectToProviderSelectionActionFilter()
        {
        }

        public int Order => 9;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor == null)
            {
                await next();
                return;
            }

            if (controllerActionDescriptor.MethodInfo.GetCustomAttribute<AllowNoProviderContextAttribute>() != null ||
                controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowNoProviderContextAttribute>() != null)
            {
                await next();
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
                var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
                var providerContext = await providerContextProvider.GetProviderContext();

                if (providerContext == null)
                {
                    context.Result = new RedirectToActionResult(
                        "Index",
                        "SearchProvider",
                        new { returnUrl = context.HttpContext.Request.GetEncodedUrl() });
                    return;
                }
                else if (isRequiresProviderContextController)
                {
                    ((IRequiresProviderContextController)context.Controller).ProviderContext = providerContext;
                }
            }

            await next();
        }
    }
}
