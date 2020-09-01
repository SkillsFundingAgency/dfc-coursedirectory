using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    /// <summary>
    /// Assigns the legacy Session-backed current provider identifier when the action is executed.
    /// </summary>
    /// <remarks>
    /// When we're transitioning from new pages to old pages, the old session-based provider context may not be set.
    /// (If it's not set correctly then navigating to old pages will either return data for the wrong provider or it
    /// will navigate to the provider selection UI.)
    /// </remarks>
    public sealed class AssignLegacyProviderContextAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

            if (!env.IsTesting())
            {
                var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
                providerContextProvider.AssignLegacyProviderContext();
            }
        }
    }
}
