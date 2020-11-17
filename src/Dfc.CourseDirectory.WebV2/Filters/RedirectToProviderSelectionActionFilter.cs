using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
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
            // If the action has a parameter of type ProviderContext and/or
            // the controller/action is decorated with RequiresProviderContextAttribute then we consider
            // provider context to be required.
            // If it's not set then redirect to the provider selection UI.

            var hasProviderContextParameter = context.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(ProviderContext))
                .Any();

            var requiresProviderContext = hasProviderContextParameter ||
                context.ActionDescriptor.Properties.ContainsKey(typeof(RequireProviderContextMarker));

            if (requiresProviderContext)
            {
                var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
                var providerContext = providerContextProvider.GetProviderContext();

                if (providerContext == null)
                {
                    context.Result = new RedirectToActionResult(
                        "Index",
                        "SearchProvider",
                        new { returnUrl = context.HttpContext.Request.GetEncodedUrl() });
                }
            }
        }
    }
}
