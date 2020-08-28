using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class RedirectToProviderSelectionActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        public RedirectToProviderSelectionActionFilter()
        {
        }

        public int Order => 9;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // If the action has a parameter of type ProviderContext and/or
            // the controller/action is decorated with RequiresProviderContextAttribute then we consider
            // provider context to be required.
            // If it's not set then redirect to the provider selection UI.

            var hasProviderContextParameter = context.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(ProviderContext))
                .Any();

            var requiresProviderContext = hasProviderContextParameter ||
                context.ActionDescriptor.Properties.ContainsKey(typeof(RequiresProviderContextMarker));

            if (hasProviderContextParameter || requiresProviderContext)
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
            }

            await next();
        }
    }
}
